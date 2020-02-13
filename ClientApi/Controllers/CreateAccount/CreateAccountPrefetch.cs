using ClientApi.Entities;
using System.Collections.Generic;

namespace ClientApi.Controllers
{
    internal class CreateAccountPrefetch
    {
        public bool IsExistingAccount { get; set; }
        public AccountType AccountType { get; set; }
        public Archetype Archetype { get; set; }
        public List<SubscriptionType> SubscriptionTypes { get; set; }
    }
}
