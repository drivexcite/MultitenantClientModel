using System;
using System.Collections.Generic;
using System.Text;

namespace ClientModel.Dtos
{
    public class DataLinkDto
    {
        public int FromSubscriptionId { get; set; }
        public int ToSubscriptionId { get; set; }
        public byte TypeDataLinkTypeId { get; set; }

        public virtual string From { get; set; }
        public virtual string To { get; set; }
        public virtual string Type { get; set; }
    }
}
