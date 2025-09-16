using System.ComponentModel.DataAnnotations;

namespace Leadway_RSA_API.DTOs
{
    public class AdminLoginDto
    {
        [Required]
        public string EmailAddress { get; set; }

        [Required]
        public string Password { get; set; }
    }
}
