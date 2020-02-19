using System;

namespace ClientModel.Entities
{
    public partial class IdentityProviderMapping
    {
        public int SubscriptionId { get; set; }
        public int IdentityProviderId { get; set; }
        public bool? Enabled { get; set; }
        public DateTime CreatedDate { get; set; }
        public string CreatedBy { get; set; }

        public virtual IdentityProvider IdentityProvider { get; set; }
        public virtual Subscription Subscription { get; set; }
    }
}
