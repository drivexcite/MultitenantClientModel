using System.ComponentModel.DataAnnotations;

namespace ClientModel.Dtos
{
    public class IdentityProviderDto
    {
        public int IdentityProviderId { get; set; }

        [Required]
        [StringLength(120, MinimumLength = 1)]
        public string Name { get; set; }
    }
}
