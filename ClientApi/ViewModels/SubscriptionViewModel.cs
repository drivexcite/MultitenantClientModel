using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace ClientApi.ViewModels
{
    public class SubscriptionViewModel
    {
        public int SubscriptionId { get; set; }

        [Required]
        [StringLength(120)]
        public string SubscriptionName { get; set; }

        public string Description { get; set; }

        public Dictionary<string, string> Tags { get; set; }

        public string OrganizationalUnit { get; set; }

        [Required]
        public byte SubscriptionTypeId { get; set; }

        public string SubscriptionType { get; set; }
    }
}
