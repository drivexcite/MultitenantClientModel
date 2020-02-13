using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace ClientApi.ViewModels
{
    public class AccountViewModel
    {
        public int AccountId { get; set; }

        [Required]
        [StringLength(120)]
        public string AccountName { get; set; }

        [Required]
        public byte AccountTypeId { get; set; }

        public string AccountType { get; set; }

        [Required]
        public byte ArchetypeId { get; set; }
        
        public string Achetype { get; set; }

        [Required]
        [StringLength(40)]
        public string SalesforceAccountId { get; set; }
        
        public string SalesforceAccountUrl { get; set; }

        [StringLength(40)]
        public string SalesforceAccountNumber { get; set; }

        [StringLength(120)]
        public string SalesforceAccountManager { get; set; }
        
        [StringLength(40)]
        public string ContractNumber { get; set; }
        
        public int SubscriptionCount { get; set; }
        
        public List<SubscriptionViewModel> Subscriptions { get; set; }
    }
}
