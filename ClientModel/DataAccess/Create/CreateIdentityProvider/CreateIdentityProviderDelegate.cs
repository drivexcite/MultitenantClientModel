using AutoMapper;
using ClientModel.Dtos;
using ClientModel.Entities;
using ClientModel.Exceptions;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using ValidationContext = System.ComponentModel.DataAnnotations.ValidationContext;

namespace ClientModel.DataAccess.Create.CreateIdentityProvider
{
    public class CreateIdentityProviderDelegate
    {
        private readonly ClientsDb _db;
        private readonly IMapper _mapper;

        public CreateIdentityProviderDelegate(ClientsDb db, IMapper mapper)
        {
            _db = db;
            _mapper = mapper;
        }

        public async Task<IdentityProviderDto> CreateIdentityProvider(int accountId, IdentityProviderDto identityProvider)
        {
            var validationErrors = new List<ValidationResult>();
            Validator.TryValidateObject(identityProvider, new ValidationContext(identityProvider, null, null), validationErrors, true);

            if (validationErrors.Count > 0)
            {
                throw new ClientModelAggregateException(validationErrors.Select((e) => new ValidationException(e.ErrorMessage)));
            }

            var account = (from a in _db.Accounts where a.AccountId == accountId select a).FirstOrDefault();
            if (account == null)
            {
                throw new AccountNotFoundException($"An account with AccountId {accountId} could not be found");
            }

            if (account.IdentityProviders.Select(p => string.Equals(p.Name, identityProvider.Name, StringComparison.OrdinalIgnoreCase)).Any())
            {
                throw new MalformedAccountException($"An identity provider with the same name [{identityProvider.Name}] already exists.");
            }

            var newIdentityProvider = new IdentityProvider
            {
                Name = identityProvider.Name,
                Account = account,
                AccountId = accountId
            };

            account.IdentityProviders.Add(newIdentityProvider);
            await _db.SaveChangesAsync();

            return _mapper.Map<IdentityProviderDto>(newIdentityProvider);
        }

        public async Task<(List<IdentityProviderDto>, int)> AssignIdentityProviderToSubscription(int accountId, int subscriptionId, List<int> identityProviderIds, int skip, int top)
        {
            var account = await (
                from a in _db.Accounts
                    .Include(a => a.IdentityProviders)
                    .Include(a => a.Subscriptions)
                    .ThenInclude(s => s.IdentityProviders)
                    .ThenInclude(m => m.IdentityProvider)
                where a.AccountId == accountId
                select a
            ).FirstOrDefaultAsync();

            if (account == null)
            {
                throw new AccountNotFoundException($"An account with AccountId {accountId} does not exist.");
            }

            var subscription = (from s in account.Subscriptions where s.SubscriptionId == subscriptionId select s).FirstOrDefault();

            if (subscription == null)
            {
                throw new SubscriptionNotFoundException($"A subscription with SubscriptionId = {subscriptionId} does not exist within Account with AccountId = {accountId}.");
            }

            foreach (var identityProviderId in identityProviderIds)
            {
                var identityProvider = account.IdentityProviders.FirstOrDefault(i => i.IdentityProviderId == identityProviderId);

                if (identityProvider == null)
                {
                    throw new MalformedAccountException($"The Identity Provider with IdentityProviderId = {identityProviderId} does not exist in the Account with AccountId = {accountId}.");
                }

                if (subscription.IdentityProviders.Any(m => m.IdentityProviderId == identityProviderId))
                {
                    throw new MalformedSubscriptionException($"The Identity Provider with IdentityProviderId = {identityProviderId} already exists in Subscription with SubscriptionId = {subscriptionId}.");
                }

                subscription.IdentityProviders.Add(new IdentityProviderMapping
                {
                    Subscription = subscription,
                    IdentityProvider = identityProvider
                });
            }

            await _db.SaveChangesAsync();
            var identityProviders = (from m in subscription.IdentityProviders select _mapper.Map<IdentityProviderDto>(m.IdentityProvider)).ToList();

            return (identityProviders.Skip(skip).Take(top).ToList(), identityProviders.Count());
        }
    }
}
