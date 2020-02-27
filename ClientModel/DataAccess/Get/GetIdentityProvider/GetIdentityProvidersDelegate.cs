using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using ClientModel.DataAccess.Common;
using ClientModel.Dtos;
using ClientModel.Entities;
using ClientModel.Exceptions;
using Microsoft.EntityFrameworkCore;

namespace ClientModel.DataAccess.Get.GetIdentityProvider
{
    public class GetIdentityProvidersDelegate
    {
        private readonly ClientsDb _db;
        private readonly IMapper _mapper;

        public GetIdentityProvidersDelegate(ClientsDb db, IMapper mapper)
        {
            _db = db;
            _mapper = mapper;
        }

        public async Task<(List<IdentityProviderDto>, int)> GetIdentityProvidersForSubscriptionAsync(int accountId, int subscriptionId, int skip, int top)
        {
            try
            {
                var account = await (
                    from a in _db.Accounts
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

                var identityProviders = (from m in subscription.IdentityProviders select m.IdentityProvider).ToList();
                var dtos = (from i in identityProviders select _mapper.Map<IdentityProviderDto>(i)).Skip(skip).Take(top).ToList();

                return (dtos, identityProviders.Count);
            }
            catch (DbException e)
            {
                throw new PersistenceException($"An error occurred while reading a DataLink ({nameof(accountId)} = {accountId}, {nameof(subscriptionId)} = {subscriptionId}, {nameof(skip)} = {skip}, {nameof(top)} = {top})", e);
            }
        }

        public virtual async Task<(List<IdentityProviderDto>, int)> GetIdentityProvidersAsync(int accountId, int skip, int top)
        {
            try
            {
                var (identityProviders, total) = await GetIdentityProvidersAndCountAsync(accountId, skip, top);
                return (await _mapper.ProjectTo<IdentityProviderDto>(identityProviders).ToListAsync(), total);
            }
            catch (DbException e)
            {
                throw new PersistenceException($"An error occurred while reading a DataLink ({nameof(accountId)} = {accountId}, {nameof(skip)} = {skip}, {nameof(top)} = {top})", e);
            }
        }

        private async Task<(IQueryable<IdentityProvider>, int)> GetIdentityProvidersAndCountAsync(int accountId, int skip, int top)
        {
            var doesAccountExist = await (from a in _db.Accounts where a.AccountId == accountId select 1).AnyAsync();
            var identityProviders = (from idp in _db.IdentityProviders where idp.AccountId == accountId select idp);

            if (!doesAccountExist)
                throw new AccountNotFoundException($"An account with AccountId {accountId} could not be found");

            return (identityProviders.Skip(skip).Take(top), identityProviders.Count());
        }

        public async Task<IdentityProviderDto> GetIdentityProviderAsync(int accountId, int identityProviderId)
        {
            try
            {
                var identityProvider = await Utils.GetIdentityProviderAsync(_db, accountId, identityProviderId);
                return _mapper.Map<IdentityProviderDto>(identityProvider);
            }
            catch (DbException e)
            {
                throw new PersistenceException($"An error occurred while reading IdentityProvider ({nameof(identityProviderId)}={identityProviderId})", e);
            }
        }
    }
}
