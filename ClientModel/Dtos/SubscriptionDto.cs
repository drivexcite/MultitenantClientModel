using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace ClientModel.Dtos
{
    public class SubscriptionDto
    {
        public int SubscriptionId { get; set; }

        [Required]
        [StringLength(120, MinimumLength = 1)]
        public string SubscriptionName { get; set; }

        public string Description { get; set; }

        public Dictionary<string, string> Tags { get; set; }

        public string OrganizationalUnit { get; set; }

        [Required]
        public byte SubscriptionTypeId { get; set; }

        public string SubscriptionType { get; set; }

        public List<IdentityProviderDto> IdentityProviders { get; set; } = new List<IdentityProviderDto>();
    }
}
