using System.ComponentModel.DataAnnotations;

namespace Leadway_RSA_API.DTOs
{
    // DTO for GET requests (the public-facing representation of an Identification)
    // Includes read-only properties like Id, ApplicantId, and UploadDate.
    public class IdentificationDto
    {
        public int Id { get; set; }
        public int ApplicantId { get; set; }
        public required string IdentificationType { get; set; }
        public required string DocumentNumber { get; set; }
        public string? ImagePath { get; set; }
        public DateTime UploadDate { get; set; }
    }

    // DTO for POST requests (creating a new Identification record)
    // This is the data the client sends to create a record.
    public class CreateIdentificationDto
    {
        // DocumentNumber is required on the model, so it should be required here too.
        [Required]
        public required string DocumentNumber { get; set; }

        // These fields from the model should also be included
        [Required]
        public required string IdentificationType { get; set; }
        public string? ImagePath { get; set; }
    }

    // DTO for PUT/PATCH requests (updating an existing Identification record)
    // All properties are optional, as a client may only update one field.
    public class UpdateIdentificationDto
    {
        public string? IdentificationType { get; set; }
        public string? DocumentNumber { get; set; }
        public string? ImagePath { get; set; }
    }

}