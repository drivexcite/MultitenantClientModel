using AutoMapper;
using ClientApi.Dtos;
using ClientModel.Entities;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ClientModel.DataAccess.GetAccount
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

        public virtual async Task<(List<AccountDto>, int)> GetAccounts(int skip, int top)
        {
            var (accounts, total) = await GetAccountsAndCountAsync(skip, top);
            return (await _mapper.ProjectTo<AccountDto>(accounts).ToListAsync(), total);
        }

        private async Task<(IQueryable<Account>, int)> GetAccountsAndCountAsync(int skip = 0, int top = 10)
        {
            return (_db.Accounts.Skip(skip).Take(top), await _db.Accounts.CountAsync());
        }
    }
}
