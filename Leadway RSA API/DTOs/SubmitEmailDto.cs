using System.ComponentModel.DataAnnotations;

namespace Leadway_RSA_API.DTOs
{
    /// <summary>
    /// A DTO used to receive just the email and sponsorship key for the first step.
    /// </summary>
    public class SubmitEmailDto
    {
        [Required(ErrorMessage = "Email address is required.")]
        [EmailAddress(ErrorMessage = "Invalid email format.")]
        public string EmailAddress { get; set; }

        [Required(ErrorMessage = "Sponsorship key is required.")]
        public string SponsorshipKey { get; set; }
    }
}
