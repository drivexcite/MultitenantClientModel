using AutoMapper;
using ClientModel.Entities;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ClientModel.Dtos;
using ClientModel.Exceptions;
using Microsoft.EntityFrameworkCore;

namespace ClientModel.DataAccess.Get.GetIdentityProviders
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

        public virtual async Task<(List<IdentityProviderDto>, int)> GetIdentityProvidersAsync(int accountId, int skip, int top)
        {
            var (identityProviders, total) = await GetIdentityProvidersAndCountAsync(accountId, skip, top);
            return (await _mapper.ProjectTo<IdentityProviderDto>(identityProviders).ToListAsync(), total);
        }

        private async Task<(IQueryable<IdentityProvider>, int)> GetIdentityProvidersAndCountAsync(int accountId, int skip, int top)
        {
            var doesAccountExist = await (from a in _db.Accounts where a.AccountId == accountId select 1).AnyAsync();
            var identityProviders = (from idp in _db.IdentityProviders where idp.AccountId == accountId select idp);

            if (!doesAccountExist)
                throw new AccountNotFoundException($"An account with AccountId {accountId} could not be found");

            return (identityProviders.Skip(skip).Take(top), identityProviders.Count());
        }
    }
}
