using ClientApi.ViewModels;
using ClientApi.Entities;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Z.EntityFramework.Plus;
using System.Text;
using Newtonsoft.Json;
using System;
using Microsoft.EntityFrameworkCore;

namespace ClientApi.Controllers
{
    class CreateAccountDelegate
    {
        internal static async Task<Account> PersistAccountAsync(AccountViewModel accountViewModel, ClientsDb db, CreateAccountPrefetch dependencies)
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
                from s in accountViewModel.Subscriptions ?? new List<SubscriptionViewModel>()
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
            db.Accounts.Add(account);

            await db.SaveChangesAsync();

            return account;
        }

        internal static async Task<(CreateAccountPrefetch, StringBuilder)> PrefetchAndValidateAsync(AccountViewModel account, ClientsDb db)
        {
            var errorMessage = new StringBuilder();

            var existingAccountFuture = (from a in db.Accounts where a.AccountId == account.AccountId || a.Name == account.AccountName select 1).DeferredAny().FutureValue();            
            var accountTypeFuture = (from t in db.AccountTypes where t.AccountTypeId == account.AccountTypeId select t).DeferredFirstOrDefault().FutureValue();
            var archetypeFuture = (from a in db.Archetypes where a.ArchetypeId == account.ArchetypeId select a).DeferredFirstOrDefault().FutureValue();

            var subscriptionTypeIds = new HashSet<byte>((from s in account.Subscriptions ?? new List<SubscriptionViewModel>() select s.SubscriptionTypeId).Distinct());
            var subscriptionTypesFuture = (from t in db.SubscriptionTypes where subscriptionTypeIds.Contains(t.SubscriptionTypeId) select t).Future();

            var subscriptionIds = new HashSet<int>((from s in account.Subscriptions where s.SubscriptionId != default select s.SubscriptionId).Distinct());
            var existingSubscriptionsFuture = (from s in db.Subscriptions where subscriptionIds.Contains(s.SubscriptionId) select s.SubscriptionId).Future();

            var existingAccount = await existingAccountFuture.ValueAsync();
            var accountType = await accountTypeFuture.ValueAsync();
            var archetype = await archetypeFuture.ValueAsync();
            var subscriptionTypes = await subscriptionTypesFuture.ToListAsync();
            var existingSubscriptions = await existingSubscriptionsFuture.ToListAsync();

            if (existingAccount)
            {
                var message = account.AccountId == default ? $"An Account with AccountName = {account.AccountName} already exists" : $"An account with the same AccountId: {account.AccountId} already exists";
                errorMessage.AppendLine(message);
            }                

            if (accountType == null)
                errorMessage.AppendLine($"The account type with AccountTypeId [{account.AccountTypeId}] is invalid");

            if (archetype == null)
                errorMessage.AppendLine($"The archetype with ArchetypeId [{account.ArchetypeId}] is invalid");

            var subscriptionErrors = (
                from s in account.Subscriptions
                where !subscriptionTypeIds.Contains(s.SubscriptionTypeId)
                select $"Invalid SubscriptionTypeId [{s.SubscriptionTypeId}] for SubscriptionId [{s.SubscriptionId}]"
            ).ToList();

            subscriptionErrors.ForEach(e => errorMessage.AppendLine(e));
            existingSubscriptions.ForEach(s => errorMessage.AppendLine($"A subscription with SubscriptionId [{s}] already exists"));

            return (new CreateAccountPrefetch { IsExistingAccount = existingAccount, AccountType = accountType, Archetype = archetype, SubscriptionTypes = subscriptionTypes }, errorMessage);
        }
    }
}
