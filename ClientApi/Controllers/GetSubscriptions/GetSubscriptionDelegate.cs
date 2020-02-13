using ClientApi.Entities;
using ClientApi.Exceptions;
using System.Linq;
using System.Threading.Tasks;
using Z.EntityFramework.Plus;

namespace ClientApi.Controllers.CreateAccount
{
    public class GetSubscriptionDelegate
    {
        private readonly ClientsDb _db;

        public GetSubscriptionDelegate(ClientsDb db)
        {
            _db = db;
        }

        public (IQueryable<Subscription>, int) GetSubscriptions(int accountId, int skip = 0, int top = 10)
        {
            var doesAccountExist = (from a in _db.Accounts where a.AccountId == accountId select 1).Any();
            var accountSubscriptions = (from s in _db.Subscriptions where s.AccountId == accountId select s);

            if (!doesAccountExist)
                throw new AccountNotFoundException($"An account with AccountId {accountId} could not be found");

            return (accountSubscriptions.Skip(skip).Take(top), accountSubscriptions.Count());
        }
    }
}
