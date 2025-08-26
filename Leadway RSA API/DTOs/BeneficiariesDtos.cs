using System.ComponentModel.DataAnnotations;
namespace Leadway_RSA_API.DTOs
{
    // DTO for GET requests
    // Includes read-only properties like Id and ApplicantId.
    public class BeneficiaryDto
    {
        public int Id { get; set; }
        public int ApplicantId { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? Gender { get; set; }
    }

    // DTO for POST requests
    // This is the data the client sends to create a new Beneficiary record.
    public class CreateBeneficiaryDto
    {
        [Required]
        public required string FirstName { get; set; }
        [Required]
        public required string LastName { get; set; }
        public string? Gender { get; set; }
    }

    // DTO for PUT/PATCH requests
    // All properties are optional, as a client may only update one field.
    public class UpdateBeneficiaryDto
    {
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? Gender { get; set; }
    }
}