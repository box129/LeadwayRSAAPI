using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;

/// <summary>
/// A Data Transfer Object (DTO) used to receive data for creating a new sponsored applicant.
/// </summary>
public class CreateSponsoredApplicantDto
{
    [Required(ErrorMessage = "First name is required.")]
    [MaxLength(50)]
    public string FirstName { get; set; }

    [Required(ErrorMessage = "Last name is required.")]
    [MaxLength(50)]
    public string LastName { get; set; }

    [Required(ErrorMessage = "Email address is required.")]
    [EmailAddress(ErrorMessage = "Invalid email format.")]
    public string EmailAddress { get; set; }

    [Phone(ErrorMessage = "Invalid phone number format.")]
    public string PhoneNumber { get; set; }

    [Required(ErrorMessage = "Sponsorship key is required.")]
    public string SponsorshipKey { get; set; }
}