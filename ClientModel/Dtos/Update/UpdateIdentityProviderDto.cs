using System.ComponentModel.DataAnnotations;

namespace ClientModel.Dtos.Update
{
    public class UpdateIdentityProviderDto
    {
        [Required]
        [StringLength(120, MinimumLength = 1)]
        public string Name { get; set; }
    }
}