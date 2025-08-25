using System.ComponentModel.DataAnnotations;

namespace Leadway_RSA_API.Models
{
    public class Asset
    {
        public int Id { get; set; } // primary key

        [Required] // An Asset record must always ne linked to an Applicant
        public int ApplicantId { get; set; } // Foreign key referencing the Applicant table's Id.

        // --- Common Asset Details ---
        [Required]
        [StringLength(50)] // e.g., "Pension", "BankAccount", "RealEstate", "Shares"
        public string AssetType { get; set; } // Categorizes the asset type. Consider using an enum for strict types.

        [Required]
        [StringLength(255)] // General name or description of the asset
        public string Name { get; set; }

        public decimal? Value { get; set; } // Optional: Estimated monetary value of the asset.
                                            // Nullable (decimal?) as some assets might not have an immediate quantifiable value,
                                            // or it might be gathered later.

        // --- Pension Specific Details (Nullable if AssetType is not "Pension") ---
        [StringLength(50)]
        public string? RSAPin { get; set; } // RSA PIN Number for pension assets (Nullable)

        [StringLength(255)]
        public string? PFA { get; set; } // Pension Fund Administrator (Nullable)

        // --- Bank Account Specific Details (Nullable if AssetType is not "BankAccount") ---
        [StringLength(100)]
        public string? BankName { get; set; } // Name of the bank (Nullable)

        [StringLength(50)]
        public string? AccountNumber { get; set; } // Bank account number (Nullable)

        [StringLength(50)]
        public string? AccountType { get; set; } // e.g., "Savings", "Current", "Domiciliary" (Nullable)

        // --- Navigation Properties (for Entity Framework Core relationships) ---

        // Navigation property to the parent Applicant (the "one" side of the one-to-many relationship)
        public virtual Applicant? Applicant { get; set; }

        // Navigation property to the BeneficiaryAssetAllocation records (the "many" side of the many-to-many relationship via the junction table)
        public virtual ICollection<BeneficiaryAssetAllocation>? AssetAllocations { get; set; } = new List<BeneficiaryAssetAllocation>();
        // This collection will hold all the specific allocations where this asset is being distributed.
    }
}
