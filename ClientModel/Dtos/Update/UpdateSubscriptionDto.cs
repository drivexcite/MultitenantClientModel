using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace ClientModel.Dtos.Update
{
    public class UpdateSubscriptionDto
    {
        [StringLength(120, MinimumLength = 1)]
        public string SubscriptionName { get; set; }

        public string Description { get; set; }

        public Dictionary<string, string> Tags { get; set; }

        [StringLength(120)]
        public string OrganizationalUnit { get; set; }
    }
}