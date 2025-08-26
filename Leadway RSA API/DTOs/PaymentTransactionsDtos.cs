using System.ComponentModel.DataAnnotations;

namespace Leadway_RSA_API.DTOs
{
    // DTO for returning data to the client
    public class PaymentTransactionDto
    {
        public int Id { get; set; }
        public int ApplicantId { get; set; }
        public decimal Amount { get; set; }
        public required string Currency { get; set; }
        public required string Status { get; set; }
        public string? GatewayReferenceId { get; set; }
        public string? PaymentMethod { get; set; }
        public DateTime TransactionDate { get; set; }
        public string? Message { get; set; }
    }

    // DTO for accepting data from the client to create a new record
    public class CreatePaymentTransactionDto
    {
        [Required]
        public decimal Amount { get; set; }

        [Required]
        [StringLength(10)]
        public string Currency { get; set; } = "NGN";

        [Required]
        [StringLength(50)]
        public required string Status { get; set; }

        [StringLength(255)]
        public string? GatewayReferenceId { get; set; }

        [StringLength(50)]
        public string? PaymentMethod { get; set; }

        [StringLength(500)]
        public string? Message { get; set; }
    }

    // DTO for accepting data from the client to update a record
    public class UpdatePaymentTransactionDto
    {
        [Required]
        [StringLength(50)]
        public required string Status { get; set; }

        [StringLength(255)]
        public string? GatewayReferenceId { get; set; }

        [StringLength(50)]
        public string? PaymentMethod { get; set; }

        [StringLength(500)]
        public string? Message { get; set; }
    }
}
