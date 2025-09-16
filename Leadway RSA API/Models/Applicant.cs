namespace Leadway_RSA_API.Models
{
    public class Applicant
    {
        public int Id { get; set; } // Primary Key

        public string? RSAPin { get; set; } // Optional: RSA Pin, can be null if not provided or required
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string PhoneNumber { get; set; }
        public string EmailAddress { get; set; }

        public DateTime DateOfBirth { get; set; }

        // -- Applicaton State and Metadata ---
        public int CurrentStep { get; set; } // Tracks the current step the applicant is on in the multi-step form.
                                             // Useful for resuming incomplete applications. E.g., 1 for Personal Details, 2 for ID, etc.
        public bool IsComplete { get; set; } // Indicates if the entire will application process is finalized (e.g., after successful payment).
        public DateTime CreatedDate { get; set; } // Timestamp of the last time any part of this applicant's record was updated.
                                                  // Automatically updated on every save.
        public DateTime LastModifiedDate { get; set; }

        // --- Navigation Properties (for Entity Framework Core relationships) ---
        public virtual PersonalDetails? PersonalDetails { get; set; }
        public virtual ICollection<Identification>? Identifications { get; set; } = new List<Identification>();
        // An Applicant can have one or more identification records (e.g., if you store historical IDs or multiple types).
        public virtual ICollection<Beneficiary>? Beneficiaries { get; set; } = new List<Beneficiary>(); // Beneficiaries are like the heirs of the will.
        public virtual ICollection<Executor>? Executors { get; set; } = new List<Executor>(); // Executors are the people who will execute the will.
        public virtual ICollection<Guardian>? Guardians { get; set; } = new List<Guardian>();
        public virtual ICollection<Asset>? Assets { get; set; } = new List<Asset>();
        public virtual ICollection<BeneficiaryAssetAllocation>? AssetAllocations { get; set; } = new List<BeneficiaryAssetAllocation>();
        // An Applicant's assets are allocated to beneficiaries. Even though this is a junction table,
        // it's useful to have a direct link from the Applicant for querying all allocations related to this specific will.
        public virtual ICollection<PaymentTransaction>? PaymentTransactions { get; set; } = new List<PaymentTransaction>();
        // An Applicant can have multiple PaymentTransactions (e.g., retries for failed payments).

        // ADDED: Navigation property for the RegistrationKey
        public virtual RegistrationKey? RegistrationKey { get; set; }
    }
}
