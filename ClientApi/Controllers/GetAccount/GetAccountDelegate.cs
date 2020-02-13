using ClientApi.Entities;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

namespace ClientApi.Controllers.CreateAccount
{
    public class GetAccountDelegate
    {
        private readonly ClientsDb _db;

        public GetAccountDelegate(ClientsDb db)
        {
            _db = db;
        }

        public async Task<(IQueryable<Account>, int)> GetAccounts(int skip = 0, int top = 10)
        {
            return (_db.Accounts.Skip(skip).Take(top), await _db.Accounts.CountAsync());
        }
    }
}
