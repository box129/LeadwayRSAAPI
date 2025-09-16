using System.ComponentModel.DataAnnotations;

namespace Leadway_RSA_API.DTOs
{
    // DTO for returning data to the client
    public class PaymentTransactionDto
    {
        public int Id { get; set; }
        public int ApplicantId { get; set; }
        public decimal? Amount { get; set; }
        public string? Currency { get; set; }
        public string? Status { get; set; }
        public string? GatewayReferenceId { get; set; }
        public string? PaymentMethod { get; set; }
        public DateTime TransactionDate { get; set; }
        public string? Message { get; set; }
    }

    // DTO for accepting data from the client to create a new record
    public class CreatePaymentTransactionDto
    {
        [Range(0.01, double.MaxValue, ErrorMessage = "Amount must be a positive value.")]
        public decimal? Amount { get; set; }

        [StringLength(10)]
        public string? Currency { get; set; } = "NGN";

        [StringLength(50)]
        public string? Status { get; set; }

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
        [StringLength(50)]
        public string? Status { get; set; }

        [StringLength(255)]
        public string? GatewayReferenceId { get; set; }

        [StringLength(50)]
        public string? PaymentMethod { get; set; }

        [StringLength(500)]
        public string? Message { get; set; }
    }
}
