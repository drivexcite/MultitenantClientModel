using System;
using System.Collections.Generic;

namespace ClientModel.Entities
{
    public partial class Subscription
    {
        public Subscription()
        {
            IdentityProviders = new HashSet<IdentityProviderMapping>();
        }

        public int SubscriptionId { get; set; }
        public int AccountId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Tags { get; set; }
        public string OrganizationalUnit { get; set; }
        public byte SubscriptionTypeId { get; set; }
        public DateTime ActivationDate { get; set; }
        public bool? Enabled { get; set; }
        public DateTime CreatedDate { get; set; }
        public string CreatedBy { get; set; }

        public virtual Account Account { get; set; }
        public virtual SubscriptionType SubscriptionType { get; set; }
        public virtual ICollection<IdentityProviderMapping> IdentityProviders { get; set; }
    }
}
