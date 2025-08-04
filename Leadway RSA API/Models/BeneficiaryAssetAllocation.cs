using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Leadway_RSA_API.Models
{
    public class BeneficiaryAssetAllocation
    {
        public int Id { get; set; } // Primary key

        [Required]
        public int ApplicantId { get; set; } // Direct foreign key to Applicant for easier querying of all allocations for a specific will.

        [Required]
        public int AssetId { get; set; } // Foreign Key referencing the Asset table's Id.
        [Required]
        public int BeneficiaryId { get; set; } // Foreign Key referencing the Beneficiary table's Id.

        // --- Allocation Details ---
        [Required]
        [Range(0.01, 100.00, ErrorMessage = "Percentage must be between 0.01 and 100.")] // Percentage should be positive and up to 100
        [Column(TypeName = "decimal(5, 2)")] // Explicitly define decimal precision and scale for database
        public decimal Percentage { get; set; } // The percentage of the Asset allocated to the Beneficiary (e.g., 50.00 for 50%).

        // --- Navigation Properties ---
        // Navigation property to the parent Applicant
        public virtual Applicant? Applicant { get; set; }

        // Navigation property to the associated Asset
        public virtual Asset? Asset { get; set; }

        // Navigation property to the associated Beneficiary
        public virtual Beneficiary? Beneficiary { get; set; }
    }
}
