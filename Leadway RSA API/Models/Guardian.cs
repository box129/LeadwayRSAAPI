using System.ComponentModel.DataAnnotations;

namespace Leadway_RSA_API.Models
{
    public class Guardian
    {
        public int Id { get; set; } // primary key

        [Required]
        public int ApplicantId { get; set; } // foreign Key referencing the applicant table's Id.

        // --- Guardian Details ---
        [Required]
        [StringLength(100)] // Limit length for first name
        public string FirstName { get; set; }

        [Required]
        [StringLength(100)] // Limit length for last name
        public string LastName { get; set; }

        [Required]
        [StringLength(20)]
        public string PhoneNumber { get; set; }

        [Required]
        [StringLength(255)]
        public string Address { get; set; }

        [Required]
        [StringLength(100)] // City name
        public string City { get; set; }

        [Required]
        [StringLength(100)] // State name (e.g., "Lagos")
        public string State { get; set; } // Consider using an Enum or lookup table for states for consistency.

        // --- Navigation Property (for Entity Framework Core relationship) ---
        // Navigation property to the parent Applicant (the "one" side of the one-to-many relationship)
        public virtual Applicant? Applicant { get; set; }
    }
}
