using Leadway_RSA_API.Services;
using System.ComponentModel.DataAnnotations;

namespace Leadway_RSA_API.Models
{
    public class Asset : IAuditable
    {
        public int Id { get; set; } // primary key

        [Required] // An Asset record must always ne linked to an Applicant
        public int ApplicantId { get; set; } // Foreign key referencing the Applicant table's Id.

        // --- Common Asset Details ---
        [Required]
        [StringLength(255)] // General name or description of the asset
        public required string Name { get; set; }

        
        // --- Pension Specific Details (Nullable if AssetType is not "Pension") ---
        [StringLength(50)]
        public string? RSAPin { get; set; } // RSA PIN Number for pension assets (Nullable)

        [StringLength(255)]
        public string? PFA { get; set; } // Pension Fund Administrator (Nullable)

        // --- Bank Account Specific Details (Nullable if AssetType is not "BankAccount") ---
        [StringLength(100)]
        public string? SalaryBankName { get; set; } // Name of the bank (Nullable)

        [StringLength(50)]
        public string? SalaryAccountNumber { get; set; } // Bank account number (Nullable)

        public DateTime CreatedDate { get; set; }
        public DateTime LastModifiedDate { get; set; }

        // --- Navigation Properties (for Entity Framework Core relationships) ---

        // Navigation property to the parent Applicant (the "one" side of the one-to-many relationship)
        public virtual Applicant? Applicant { get; set; }

        // Navigation property to the BeneficiaryAssetAllocation records (the "many" side of the many-to-many relationship via the junction table)
        public virtual ICollection<BeneficiaryAssetAllocation>? AssetAllocations { get; set; } = new List<BeneficiaryAssetAllocation>();
        // This collection will hold all the specific allocations where this asset is being distributed.
    }
}
