using Leadway_RSA_API.Services;
using System.ComponentModel.DataAnnotations;

namespace Leadway_RSA_API.Models
{
    public class Beneficiary : IAuditable
    {
        public int Id { get; set; } // Primary Key

        [Required]
        public int ApplicantId { get; set; } // Forign Key referencing the Applicant table's Id.

        // --- Beneficiary Details ---
        [Required]
        [StringLength(100)] // Limit length for first name
        public required string FirstName { get; set; }

        [Required]
        [StringLength(100)] // Limit length for last name
        public required string LastName { get; set; }

        [Required]
        [DataType(DataType.Date)]
        public DateTime DateOfBirth { get; set; }

        [Required]
        [StringLength(20)]
        public required string Gender { get; set; }

        public DateTime CreatedDate { get; set; }
        public DateTime LastModifiedDate { get; set; }

        // --- Navigation Properties (for Entity Framework Core relationships) ---
        public virtual Applicant? Applicant { get; set; }

        // Navigation property to the BeneficiaryAssetAllocation records (the "many" side of the many-to-many relationship via the junction table)
        public virtual ICollection<BeneficiaryAssetAllocation>? AssetAllocations { get; set; } = new List<BeneficiaryAssetAllocation>();
        // This collection will hold all the specific allocations where this beneficiary is receiving an asset.
    }
}