using ClientApi.Entities;
using System.Linq;

namespace ClientApi.Controllers.CreateAccount
{
    public class GetAccountDelegate
    {
        private readonly ClientsDb _db;

        public GetAccountDelegate(ClientsDb db)
        {
            _db = db;
        }

        public (IQueryable<Account>, int) GetAccounts(int skip = 0, int top = 10)
        {
            return (_db.Accounts.Skip(skip).Take(top), _db.Accounts.Count());
        }
    }
}
