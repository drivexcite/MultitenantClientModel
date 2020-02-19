using System;

namespace ClientModel.Entities
{
    public partial class AccountType
    {
        public byte AccountTypeId { get; set; }
        public string Name { get; set; }
        public DateTime CreatedDate { get; set; }
        public string CreatedBy { get; set; }
    }
}
