using System.ComponentModel.DataAnnotations;

namespace Leadway_RSA_API.DTOs
{
    public class CreateLoginDto
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        public string Password { get; set; }
    }
}
