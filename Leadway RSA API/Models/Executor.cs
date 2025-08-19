using Leadway_RSA_API.CustomValidators;
using System.ComponentModel.DataAnnotations;

namespace Leadway_RSA_API.Models
{
    public class Executor
    {
        // Apply the custom validation attribute at the class level
        [ExecutorTypeValidation]
        public int Id { get; set; } // primary key

        [Required]
        public int ApplicantId { get; set; } // Foreign Key referencing thre applicabt table Id

        // --- Executor Details ---
        [Required]
        [StringLength(50)] // "Individual or Company"
        public required string ExecutorType { get; set; }

        [StringLength(100)]
        public string? FirstName { get; set; } // Nullable if ExecutorType is "Company"

        [StringLength(100)]
        public string? LastName { get; set; }  // Nullable if ExecutorType is "Company"

        // --- Details for Company Executor (Used if ExecutorType is "Company") ---
        [StringLength(255)]
        public string? CompanyName { get; set; } // Nullable if ExecutorType is "Individual"

        // --- Common Details for both Individual and Company Executors ---
        [Required]
        [StringLength(20)]
        public required string PhoneNumber { get; set; }

        [Required]
        [StringLength(255)] // Sufficient length for a street address
        public required string Address { get; set; }

        [Required]
        [StringLength(100)] // City name
        public required string City { get; set; }

        [Required]
        [StringLength(100)] // State name (e.g., "Lagos")
        public required string State { get; set; } // Consider using an Enum or lookup table for states for consistency.

        // --- Navigation Property (for Entity Framework Core relationship) ---
        // Navigation property to the parent Applicant (the "one" side of the one-to-many relationship)
        public virtual Applicant? Applicant { get; set; }
    }
}
