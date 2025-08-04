using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Leadway_RSA_API.Models
{
    public class PaymentTransaction
    {
        public int Id { get; set; } // primary Key

        [Required] // A PaymentTransaction record must always be linked to an Applicant
        public int ApplicantId { get; set; } // Foreign Key referencing the Applicant table's Id.

        // --- Payment Details ---
        [Required]
        [Column(TypeName = "decimal(18, 2)")] // Explicitly define decimal precision and scale for currency
        public decimal Amount { get; set; } // The amount of the transaction (e.g., 40000.00)

        [Required]
        [StringLength(10)] // e.g., "NGN", "USD"
        public string Currency { get; set; } = "NGN"; // Default to Nigerian Naira

        [Required]
        [StringLength(50)] // "Pending", "Success", "Failed", "Cancelled"
        public required string Status { get; set; } // Current status of the payment transaction

        [StringLength(255)] // Unique ID provided by the payment gateway
        public string? GatewayReferenceId { get; set; } // Nullable initially, populated after gateway response

        [StringLength(50)] // e.g., "Card", "BankTransfer", "USSD"
        public string? PaymentMethod { get; set; } // Nullable if not always captured or known upfront

        public DateTime TransactionDate { get; set; } = DateTime.UtcNow; // Timestamp when the transaction record was created/initiated.
                                                                         // Automatically set to UTC Now when record is added.

        [StringLength(500)] // Optional message from the payment gateway or for internal notes
        public string? Message { get; set; } // e.g., success message, error details

        // --- Navigation Property ---
        // Navigation property to the parent Applicant (the "one" side of the one-to-many relationship)
        public virtual Applicant? Applicant { get; set; }
    }
}

