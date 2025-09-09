using System.ComponentModel.DataAnnotations;

namespace Leadway_RSA_API.DTOs
{
    public class CreatePersonalDetailsDto
    {
        [Required]
        public required string PlaceOfBirth { get; set; }
        [Required]
        public required string Religion { get; set; }
        [Required]
        public required string Gender { get; set; }
        [Required]
        public required string HomeAddress { get; set; }
        [Required]
        public required string State { get; set; }
        [Required]
        public required string City { get; set; }
        // We do not include file paths here as the files will be uploaded separately
        // and their paths will be set by the service layer.
    }

    public class PersonalDetailsDto
    {
        public int Id { get; set; }
        public int ApplicantId { get; set; }
        public required string PlaceOfBirth { get; set; }
        public required string Religion { get; set; }
        public required string Gender { get; set; }
        public required string HomeAddress { get; set; }
        public required string State { get; set; }
        public required string City { get; set; }
        public string? PassportPhotoPath { get; set; }
        public string? SignaturePath { get; set; }
    }

    public class UpdatePersonalDetailsDto
    {
        public string? PlaceOfBirth { get; set; }
        public string? Religion { get; set; }
        public string? Gender { get; set; }
        public string? HomeAddress { get; set; }
        public string? State { get; set; }
        public string? City { get; set; }
    }
}
