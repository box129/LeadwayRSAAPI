using Leadway_RSA_API.CustomValidators;
using Leadway_RSA_API.Services;
using System.ComponentModel.DataAnnotations;

namespace Leadway_RSA_API.Models
{
    public enum ExecutorType
    {
        Individual,
        Company
    }

    [ExecutorTypeValidation] // Custom Validation
    public class Executor : IAuditable
    {
        // Apply the custom validation attribute at the class level
        
        public int Id { get; set; } // primary key

        [Required]
        public int ApplicantId { get; set; } // Foreign Key referencing thre applicabt table Id

        // We use a boolean to indicate if this is the default executor.
        public bool IsDefault { get; set; }

        [StringLength(100)]
        public string? Name { get; set; } // Used for the default executor ("Leadway Trustees")


        // --- Details for user-added executors ---
        // These are only used if IsDefault is false.
        public ExecutorType? ExecutorType { get; set; } // Nullable because it's not applicable for the default executor.

        [StringLength(100)]
        public string? FirstName { get; set; }

        [StringLength(100)]
        public string? LastName { get; set; }

        [StringLength(255)]
        public string? CompanyName { get; set; }

        // --- Common details for all executors ---
        [Required]
        [StringLength(20)]
        public required string PhoneNumber { get; set; }

        [Required]
        [StringLength(255)]
        public required string Address { get; set; }

        [Required]
        [StringLength(100)]
        public required string City { get; set; }

        [Required]
        [StringLength(100)]
        public required string State { get; set; }

        public DateTime CreatedDate { get; set; }
        public DateTime LastModifiedDate { get; set; }
        public virtual Applicant? Applicant { get; set; }
    }
}
