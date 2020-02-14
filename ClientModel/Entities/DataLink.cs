using System;
using System.Collections.Generic;

namespace ClientModel.Entities
{
    public partial class DataLink
    {
        public int FromSubscriptionId { get; set; }
        public int ToSubscriptionId { get; set; }
        public byte TypeDataLinkTypeId { get; set; }
        public DateTime CreatedDate { get; set; }
        public string CreatedBy { get; set; }

        public virtual Subscription From { get; set; }
        public virtual Subscription To { get; set; }
        public virtual DataLinkType Type { get; set; }
    }
}
