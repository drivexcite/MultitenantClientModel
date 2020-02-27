using System.ComponentModel.DataAnnotations;

namespace ClientModel.Dtos.Update
{
    public class UpdateAccountDto
    {
        public string AccountName { get; set; }

        public byte AccountTypeId { get; set; }

        public byte ArchetypeId { get; set; }

        [StringLength(40, MinimumLength = 1)]
        public string SalesforceAccountId { get; set; }

        public string SalesforceAccountUrl { get; set; }

        [StringLength(40, MinimumLength = 1)]
        public string SalesforceAccountNumber { get; set; }

        [StringLength(120, MinimumLength = 1)]
        public string SalesforceAccountManager { get; set; }

        [StringLength(40, MinimumLength = 1)]
        public string ContractNumber { get; set; }
    }
}