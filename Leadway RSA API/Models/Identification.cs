using System.ComponentModel.DataAnnotations;

namespace Leadway_RSA_API.Models
{
    public class Identification
    {
        public int Id { get; set; } // Primary Key

         // An Identification record must always be linked to an Applicant
        public int ApplicantId { get; set; } // Foreign Key

        [Required]
        [StringLength(100)] // Limit length for dcument number
        public string? DocumentNumber { get; set; } // The actual license/document number

        [StringLength(500)] // Store path/URL, not binary image itself.
        public string? ImagePath { get; set; }
        public DateTime UploadDate { get; set; } // = DateTime.UtcNow; // Timestamp when the identification was uploaded/recorded.

        // --- Navigation Property (for Entity Framework Core relationship) ---
        // This property represents the "one" side of the one-to-many relationship with Applicant.
        public virtual Applicant? Applicant { get; set; }
    }
}
