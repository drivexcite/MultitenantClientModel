using System;

namespace ClientModel.Entities
{
    public partial class IdentityProvider
    {
        public int IdentityProviderId { get; set; }
        public string Name { get; set; }
        public int AccountId { get; set; }
        public DateTime CreatedDate { get; set; }
        public string CreatedBy { get; set; }

        public virtual Account Account { get; set; }
    }
}
