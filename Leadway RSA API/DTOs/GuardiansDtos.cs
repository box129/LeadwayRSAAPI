using System.ComponentModel.DataAnnotations;
namespace Leadway_RSA_API.DTOs
{
    public class GuardiansDtos
    {
        public int Id { get; set; }
        public int ApplicantId { get; set; }
        public required string FirstName { get; set; }
        public required string LastName { get; set; }
        public required string PhoneNumber { get; set; }
        public required string Relationship { get; set; }
        public required string Address { get; set; }
        public required string? City { get; set; }
        public required string? State { get; set; }
    }

    public class CreateGuardiansDtos
    {
        [Required]
        public required string FirstName { get; set; }
        [Required]
        public required string LastName { get; set; }
        [Required]
        public required string PhoneNumber { get; set; }
        [Required]
        public required string Address { get; set; }
        [Required]
        public required string Relationship { get; set; }
        [Required]
        public required string City { get; set; }
        [Required]
        public required string State { get; set; }
    }

    public class UpdateGuardiansDtos
    {
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? PhoneNumber { get; set; }
        public string? Address { get; set; }
        public string? Relationship { get; set; }
        public string? City { get; set; }
        public string? State { get; set; }
    }
}
