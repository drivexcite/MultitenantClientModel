using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Z.EntityFramework.Plus;
using Newtonsoft.Json;
using System;
using ClientModel.Entities;
using ClientApi.Dtos;
using ClientModel.Exceptions;
using AutoMapper;
using System.ComponentModel.DataAnnotations;
using ValidationContext = System.ComponentModel.DataAnnotations.ValidationContext;

namespace ClientModel.DataAccess.CreateAccount
{
    public class CreateAccountDelegate
    {
        private readonly ClientsDb _db;
        private readonly IMapper _mapper;

        public CreateAccountDelegate(ClientsDb db, IMapper mapper)
        {
            _db = db;
            _mapper = mapper;
        }

        public virtual async Task<AccountDto> CreateAccount(AccountDto accountViewModel)
        {
            var validationErrors = new List<ValidationResult>();

            if (!Validator.TryValidateObject(accountViewModel, new ValidationContext(accountViewModel, null, null), validationErrors, true))
            {
                throw new AggregateException(validationErrors.Select((e) => new ValidationException(e.ErrorMessage)));
            }

            var dependencies = await PrefetchAndValidateAsync(accountViewModel);
            var account = await PersistAccountAsync(accountViewModel, dependencies);

            return _mapper.Map<AccountDto>(account);
        }

        private async Task<Account> PersistAccountAsync(AccountDto accountViewModel, CreateAccountPrefetch dependencies)
        {
            var account = new Account
            {
                AccountId = accountViewModel.AccountId,
                Name = accountViewModel.AccountName,
                AccountTypeId = dependencies.AccountType.AccountTypeId,
                AccountType = dependencies.AccountType,
                Archetype = dependencies.Archetype,
                ArchetypeId = dependencies.Archetype.ArchetypeId,
                SalesforceAccountId = accountViewModel.SalesforceAccountId,
                SalesforceAccountUrl = accountViewModel.SalesforceAccountUrl,
                SalesforceAccountNumber = accountViewModel.SalesforceAccountNumber,
                SalesforceAccountManager = accountViewModel.SalesforceAccountManager,
                ContractNumber = accountViewModel.ContractNumber
            };

            var subscriptions = (
                from s in accountViewModel.Subscriptions ?? new List<SubscriptionDto>()
                select new Subscription
                {
                    SubscriptionId = s.SubscriptionId,
                    Account = account,
                    Name = s.SubscriptionName,
                    Description = s.Description,
                    Tags = s.Tags == null || s.Tags.Keys.Count < 1 ? null : JsonConvert.SerializeObject(s.Tags),
                    OrganizationalUnit = s.OrganizationalUnit,
                    SubscriptionTypeId = s.SubscriptionTypeId,
                    SubscriptionType = dependencies.SubscriptionTypes.First(t => t.SubscriptionTypeId == s.SubscriptionTypeId),
                    ActivationDate = DateTime.UtcNow,
                    Enabled = true
                }
            ).ToList();

            subscriptions.ForEach(account.Subscriptions.Add);
            _db.Accounts.Add(account);

            await _db.SaveChangesAsync();

            return account;
        }

        private async Task<CreateAccountPrefetch> PrefetchAndValidateAsync(AccountDto account)
        {
            var exceptions = new List<Exception>();

            try
            {
                var existingAccountFuture = (from a in _db.Accounts where a.AccountId == account.AccountId || a.Name == account.AccountName select 1).DeferredAny().FutureValue();
                var accountTypeFuture = (from t in _db.AccountTypes where t.AccountTypeId == account.AccountTypeId select t).DeferredFirstOrDefault().FutureValue();
                var archetypeFuture = (from a in _db.Archetypes where a.ArchetypeId == account.ArchetypeId select a).DeferredFirstOrDefault().FutureValue();

                var subscriptionTypeIds = new HashSet<byte>((from s in account.Subscriptions ?? new List<SubscriptionDto>() select s.SubscriptionTypeId).Distinct());
                var subscriptionTypesFuture = (from t in _db.SubscriptionTypes where subscriptionTypeIds.Contains(t.SubscriptionTypeId) select t).Future();

                var subscriptionIds = new HashSet<int>((from s in account.Subscriptions where s.SubscriptionId != default select s.SubscriptionId).Distinct());
                var existingSubscriptionsFuture = (from s in _db.Subscriptions where subscriptionIds.Contains(s.SubscriptionId) select s.SubscriptionId).Future();

                var existingAccount = await existingAccountFuture.ValueAsync();
                var accountType = await accountTypeFuture.ValueAsync();
                var archetype = await archetypeFuture.ValueAsync();
                var subscriptionTypes = await subscriptionTypesFuture.ToListAsync();
                var existingSubscriptions = await existingSubscriptionsFuture.ToListAsync();

                if (existingAccount)
                {
                    var message = account.AccountId == default ? $"An Account with {nameof(AccountDto.AccountName)} = {account.AccountName} already exists" : $"An account with the same AccountId: {account.AccountId} already exists";
                    exceptions.Add(new ExistingAccountException(message));
                }

                if (accountType == null)
                    exceptions.Add(new MalformedAccountException($"The account type with {nameof(AccountDto.AccountTypeId)} [{account.AccountTypeId}] is invalid"));

                if (archetype == null)
                    exceptions.Add(new MalformedAccountException($"The archetype with {nameof(AccountDto.ArchetypeId)} [{account.ArchetypeId}] is invalid"));

                var subscriptionErrors = (
                    from s in account.Subscriptions
                    where !subscriptionTypeIds.Contains(s.SubscriptionTypeId)
                    select new MalformedSubscriptionsException($"Invalid {nameof(SubscriptionDto.SubscriptionTypeId)} [{s.SubscriptionTypeId}] for SubscriptionId [{s.SubscriptionId}]")
                ).ToList();

                subscriptionErrors.ForEach(exceptions.Add);
                existingSubscriptions.ForEach(s => exceptions.Add(new MalformedSubscriptionsException($"A subscription with {nameof(SubscriptionDto.SubscriptionTypeId)} [{s}] already exists")));

                if (exceptions.Count > 0)
                    throw new AggregateException("Some errors where found in the graph of the Account object.", exceptions);

                return new CreateAccountPrefetch
                {
                    IsExistingAccount = existingAccount,
                    AccountType = accountType,
                    Archetype = archetype,
                    SubscriptionTypes = subscriptionTypes
                };
            }
            catch (AggregateException e)
            {
                throw e;
            }
            catch (Exception e)
            {
                throw new PersistenceException("An unexpected error ocurred while validating the Create Account request.", e);
            }
        }
    }

    internal class CreateAccountPrefetch
    {
        public bool IsExistingAccount { get; set; }
        public AccountType AccountType { get; set; }
        public Archetype Archetype { get; set; }
        public List<SubscriptionType> SubscriptionTypes { get; set; }
    }
}
