﻿using System;

namespace ClientModel.Entities
{
    public partial class DataLinkType
    {
        public byte DataLinkTypeId { get; set; }
        public string Name { get; set; }
        public DateTime CreatedDate { get; set; }
        public string CreatedBy { get; set; }
    }
}
