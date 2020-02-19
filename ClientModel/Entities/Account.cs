using System;
using System.Collections.Generic;

namespace ClientModel.Entities
{
    public partial class Account
    {
        public Account()
        {
            Subscriptions = new HashSet<Subscription>();
            IdentityProviders = new HashSet<IdentityProvider>();
        }

        public int AccountId { get; set; }
        public string Name { get; set; }
        public byte AccountTypeId { get; set; }
        public byte ArchetypeId { get; set; }
        public string SalesforceAccountId { get; set; }
        public string SalesforceAccountUrl { get; set; }
        public string SalesforceAccountNumber { get; set; }
        public string SalesforceAccountManager { get; set; }
        public string ContractNumber { get; set; }
        public bool? Enabled { get; set; }
        public DateTime CreatedDate { get; set; }
        public string CreatedBy { get; set; }

        public virtual AccountType AccountType { get; set; }
        public virtual Archetype Archetype { get; set; }
        public virtual ICollection<Subscription> Subscriptions { get; set; }
        public virtual ICollection<IdentityProvider> IdentityProviders { get; set; }
    }
}
