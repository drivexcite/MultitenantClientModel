using System;
using System.Collections.Generic;

namespace ClientModel.Entities
{
    public partial class Archetype
    {
        public byte ArchetypeId { get; set; }
        public string Name { get; set; }
        public DateTime CreatedDate { get; set; }
        public string CreatedBy { get; set; }
    }
}
