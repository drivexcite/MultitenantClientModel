using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data.Common;
using System.Linq;
using System.Threading.Tasks;
using ClientModel.Dtos;
using ClientModel.Entities;
using ClientModel.Exceptions;
using Microsoft.EntityFrameworkCore;
using Z.EntityFramework.Plus;

namespace ClientModel.DataAccess.Common
{
    internal class Utils
    {
        public static void ValidateDto<T>(T dto, List<ValidationResult> validationErrors) where T : class
        {
            Validator.TryValidateObject(dto, new ValidationContext(dto, null, null), validationErrors, true);
        }

        public static void ThrowAggregateExceptionOnValidationErrors(List<ValidationResult> validationErrors)
        {
            if (validationErrors.Count > 0)
            {
                throw new ClientModelAggregateException(validationErrors.Select(e => new ValidationException(e.ErrorMessage)));
            }
        }

        public static async Task<Account> GetAccountAsync(ClientsDb db, int accountId)
        {
            try
            {
                var account = await (
                    from a in db.Accounts
                        .Include(a => a.IdentityProviders)
                        .Include(a => a.Subscriptions)
                        .ThenInclude(s => s.IdentityProviders)
                        .ThenInclude(m => m.IdentityProvider)
                    where a.AccountId == accountId
                    select a
                ).FirstOrDefaultAsync();

                if (account == null)
                    throw new AccountNotFoundException($"An account with AccountId = {accountId} could not be found.");

                return account;
            }
            catch (DbException e)
            {
                throw new PersistenceException($"An error occurred while reading Account ({nameof(accountId)}={accountId})", e);
            }
        }

        public static async Task<IdentityProvider> GetIdentityProviderAsync(ClientsDb db, int accountId, int identityProviderId)
        {
            try
            {
                var identityProvider = await (
                    from p in db.IdentityProviders
                    where p.IdentityProviderId == identityProviderId
                        && p.AccountId == accountId
                    select p
                ).FirstOrDefaultAsync();

                if (identityProvider == null)
                    throw new IdentityProviderNotFoundException($"An identity provider with {nameof(IdentityProvider.IdentityProviderId)} = {identityProviderId} could not be found.");

                return identityProvider;
            }
            catch (DbException e)
            {
                throw new PersistenceException($"An error occurred while reading IdentityProvider ({nameof(identityProviderId)}={identityProviderId})", e);
            }
        }

        public static async Task<Subscription> GetSubscriptionAsync(ClientsDb db, int accountId, int subscriptionId)
        {
            try
            {
                var doesAccountExistsFuture = (
                    from a in db.Accounts
                    where a.AccountId == accountId
                    select 1
                ).DeferredCount().FutureValue();

                var subscriptionFuture = (
                    from s in db.Subscriptions
                        .Include(s => s.IdentityProviders)
                        .ThenInclude(m => m.IdentityProvider)
                    where s.AccountId == accountId
                          && s.SubscriptionId == subscriptionId
                    select s
                ).DeferredFirstOrDefault().FutureValue();

                var doesAccountExist = await doesAccountExistsFuture.ValueAsync() > 0;
                var subscription = await subscriptionFuture.ValueAsync();

                if (!doesAccountExist)
                    throw new AccountNotFoundException($"An account with AccountId {accountId} could not be found");

                if (subscription == null)
                    throw new SubscriptionNotFoundException($"A subscription with SubscriptionId {subscriptionId} could not be found");

                return subscription;
            }
            catch (DbException e)
            {
                throw new PersistenceException($"An error occurred while reading a DataLink ({nameof(accountId)} = {accountId}, {nameof(subscriptionId)} = {subscriptionId})", e);
            }
        }
    }
}
