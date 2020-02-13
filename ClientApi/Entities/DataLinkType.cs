using System;
using System.Collections.Generic;

namespace ClientApi.Entities
{
    public partial class DataLinkType
    {
        public byte DataLinkTypeId { get; set; }
        public string Name { get; set; }
        public DateTime CreatedDate { get; set; }
        public string CreatedBy { get; set; }
    }
}
