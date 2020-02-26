using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using ClientModel.Dtos;
using ClientModel.Entities;
using ClientModel.Exceptions;
using Microsoft.EntityFrameworkCore;

namespace ClientModel.DataAccess.Get.GetAccount
{
    public class GetAccountDelegate
    {
        private readonly ClientsDb _db;
        private readonly IMapper _mapper;

        public GetAccountDelegate(ClientsDb db, IMapper mapper)
        {
            _db = db;
            _mapper = mapper;
        }

        public virtual async Task<AccountDto> GetAccountAsync(int accountId)
        {
            try
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
                    throw new AccountNotFoundException($"An account with AccountId = {accountId} could not be found.");

                return _mapper.Map<AccountDto>(account);
            }
            catch (DbException e)
            {
                throw new PersistenceException($"An error occurred while reading Account ({nameof(accountId)}={accountId})", e);
            }
        }

        public virtual async Task<(List<AccountDto>, int)> GetAccountsAsync(int skip, int top)
        {
            try
            {
                var (accounts, total) = await GetAccountsAndCountAsync(skip, top);
                return (await _mapper.ProjectTo<AccountDto>(accounts).ToListAsync(), total);
            }
            catch (DbException e)
            {
                throw new PersistenceException($"An error occurred while reading Accounts ({nameof(skip)}={skip}, {nameof(top)}={top})", e);
            }
        }

        private async Task<(IQueryable<Account>, int)> GetAccountsAndCountAsync(int skip = 0, int top = 10)
        {
            return (_db.Accounts.Skip(skip).Take(top), await _db.Accounts.CountAsync());
        }
    }
}
