using System.ComponentModel.DataAnnotations;

namespace ClientModel.Dtos
{
    public class DataLinkDto
    {
        [Required]
        public int FromSubscriptionId { get; set; }

        [Required]
        public int ToSubscriptionId { get; set; }

        [Required]
        public byte DataLinkTypeId { get; set; }

        public virtual string From { get; set; }
        public virtual string To { get; set; }
        public virtual string Type { get; set; }
    }
}
