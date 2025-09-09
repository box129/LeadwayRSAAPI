using System.ComponentModel.DataAnnotations;

namespace Leadway_RSA_API.DTOs
{
    // DTO for GET requests
    // Includes read-only properties like Id and ApplicantId.
    public class AssetDto
    {
        public int Id { get; set; } // Primary
        public int ApplicantId { get; set; } // Foreign 
        public required string Name { get; set; }
        public string? RSAPin { get; set; }
        public string? PFA { get; set; }
        public string? SalaryBankName { get; set; }
        public string? SalaryAccountNumber { get; set; }
    }

    // DTO for POST requests
    // This is the data the client sends to create a new Asset record.
    public class CreateAssetDto
    {
        [Required]
        public required string Name { get; set; }
        public string? RSAPin { get; set; }
        public string? PFA { get; set; }
        public string? SalaryBankName { get; set; }
        public string? SalaryAccountNumber { get; set; }
    }

    // DTO for PUT/PATCH requests
    // All properties are optional, as a client may only update one field.
    public class UpdateAssetDto
    {
        public string? Name { get; set; }
        public string? RSAPin { get; set; }
        public string? PFA { get; set; }
        public string? SalaryBankName { get; set; }
        public string? SalaryAccountNumber { get; set; }
    }
}