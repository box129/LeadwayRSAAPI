using Leadway_RSA_API.Services;
using System.ComponentModel.DataAnnotations;

namespace Leadway_RSA_API.Models
{
    public enum IdentificationType
    {
        Passport,
        DriversLicense,
        VotersCard,
        BVN
    }
    public class Identification : IAuditable
    {
        public int Id { get; set; } // Primary Key

        // An Identification record must always be linked to an Applicant
        public int ApplicantId { get; set; } // Foreign Key

        [Required]
        public IdentificationType IdentificationType { get; set; }
        
        [StringLength(100)] // Limit length for dcument number
        public string? DocumentNumber { get; set; } // The actual license/document number

        [StringLength(500)] // Store path/URL, not binary image itself.
        public string? ImagePath { get; set; }
        public DateTime UploadDate { get; set; } = DateTime.UtcNow; // Timestamp when the identification was uploaded/recorded.

        public DateTime CreatedDate { get; set; }
        public DateTime LastModifiedDate { get; set; }

        // --- Navigation Property (for Entity Framework Core relationship) ---
        // This property represents the "one" side of the one-to-many relationship with Applicant.
        public virtual Applicant? Applicant { get; set; }
    }
}
