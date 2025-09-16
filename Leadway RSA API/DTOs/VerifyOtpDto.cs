using System.ComponentModel.DataAnnotations;

namespace Leadway_RSA_API.DTOs
{
    /// <summary>
    /// A DTO used to verify the email with the OTP.
    /// </summary>
    public class VerifyOtpDto
    {
        [Required(ErrorMessage = "Email address is required.")]
        [EmailAddress(ErrorMessage = "Invalid email format.")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Sponsorship key is required.")]
        public string SponsorshipKey { get; set; }

        [Required(ErrorMessage = "OTP is required.")]
        public string Otp { get; set; }
    }
}
