using System.ComponentModel.DataAnnotations;

namespace Leadway_RSA_API.DTOs
{
    public class ResendKeyDto
    {
        [Required]
        [EmailAddress]
        public string EmailAddress { get; set; }
    }
}
