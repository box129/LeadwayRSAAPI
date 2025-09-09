using System.ComponentModel.DataAnnotations;

namespace Leadway_RSA_API.Models
{
    public class Login
    {
        [Required]
        public string password { get; set; }
        [Required]
        [EmailAddress]
        public string Email { get; set; }
    }
}
