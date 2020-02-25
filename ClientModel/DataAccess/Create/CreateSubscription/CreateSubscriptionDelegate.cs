using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data.Common;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using ClientModel.Dtos;
using ClientModel.Entities;
using ClientModel.Exceptions;
using Newtonsoft.Json;
using Z.EntityFramework.Plus;
using ValidationContext = System.ComponentModel.DataAnnotations.ValidationContext;

namespace ClientModel.DataAccess.Create.CreateSubscription
{
    public class CreateSubscriptionDelegate
    {
        private readonly ClientsDb _db;
        private readonly IMapper _mapper;

        public CreateSubscriptionDelegate(ClientsDb db, IMapper mapper)
        {
            _db = db;
            _mapper = mapper;
        }

        public virtual async Task<SubscriptionDto> CreateAccountAsync(int accountId, SubscriptionDto subscriptionDto)
        {
            var validationErrors = new List<ValidationResult>();

            Validator.TryValidateObject(subscriptionDto, new ValidationContext(subscriptionDto, null, null), validationErrors, true);

            if (validationErrors.Count > 0)
            {
                throw new ClientModelAggregateException(validationErrors.Select((e) => new ValidationException(e.ErrorMessage)));
            }

            var (account, subscriptionType) = await PrefetchAndValidateAsync(accountId, subscriptionDto);
            var subscription = await PersistSubscriptionAsync(account, subscriptionType, subscriptionDto);

            return _mapper.Map<SubscriptionDto>(subscription);
        }

        private async Task<(Account, SubscriptionType)> PrefetchAndValidateAsync(int accountId, SubscriptionDto subscription)
        {
            try
            {
                var existingAccountFuture = (from a in _db.Accounts where a.AccountId == accountId select a).DeferredFirstOrDefault().FutureValue();
                var subscriptionTypeFuture = (from t in _db.SubscriptionTypes where t.SubscriptionTypeId == subscription.SubscriptionTypeId select t).DeferredFirstOrDefault().FutureValue();
                var existingSubscriptionsFuture = (from s in _db.Subscriptions where s.SubscriptionId == subscription.SubscriptionId select 1).DeferredCount().FutureValue();

                var account = await existingAccountFuture.ValueAsync();
                var subscriptionType = await subscriptionTypeFuture.ValueAsync();
                
                if(account == null)
                {
                    throw new AccountNotFoundException($"An account with AccountId {accountId} could not be found.");
                }

                if (await existingSubscriptionsFuture.ValueAsync() > 1)
                {
                    throw new MalformedSubscriptionException($"An existing subscription with Subscription {subscription.SubscriptionId} already exists.");
                }

                if (subscriptionType == null)
                {
                    throw new MalformedSubscriptionException($"A subscription type with SubscriptionTypeId {subscription.SubscriptionTypeId} doesn't exist.");
                }

                return (account, subscriptionType);
            }
            catch (DbException e)
            {
                throw new PersistenceException("An unexpected error occurred while validating the Create Account request.", e);
            }
        }

        private async Task<Subscription> PersistSubscriptionAsync(Account account, SubscriptionType subscriptionType, SubscriptionDto subscriptionDto)
        {
            var subscription = new Subscription
            {
                SubscriptionId = subscriptionDto.SubscriptionId,
                Account = account,
                AccountId = account.AccountId,
                Name = subscriptionDto.SubscriptionName,
                Description = subscriptionDto.Description,
                Tags = subscriptionDto.Tags == null || subscriptionDto.Tags.Keys.Count < 1 ? null : JsonConvert.SerializeObject(subscriptionDto.Tags),
                OrganizationalUnit = subscriptionDto.OrganizationalUnit,
                SubscriptionTypeId = subscriptionDto.SubscriptionTypeId,
                SubscriptionType = subscriptionType,
                ActivationDate = DateTime.UtcNow,
                Enabled = true
            };

            account.Subscriptions.Add(subscription);
            await _db.SaveChangesAsync();

            return subscription;
        }
    }
}