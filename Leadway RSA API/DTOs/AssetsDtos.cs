using System.ComponentModel.DataAnnotations;

namespace Leadway_RSA_API.DTOs
{
    // DTO for GET requests
    // Includes read-only properties like Id and ApplicantId.
    public class AssetDto
    {
        public int Id { get; set; } // Primary
        public int ApplicantId { get; set; } // Foreign 
        public string AssetType { get; set; }
        public string Name { get; set; }
        public decimal? Value { get; set; }
        public string? RSAPin { get; set; }
        public string? PFA { get; set; }
        public string? BankName { get; set; }
        public string? AccountNumber { get; set; }
        public string? AccountType { get; set; }
    }

    // DTO for POST requests
    // This is the data the client sends to create a new Asset record.
    public class CreateAssetDto
    {
        [Required]
        public string AssetType { get; set; }
        [Required]
        public string Name { get; set; }
        public decimal? Value { get; set; }
        public string? RSAPin { get; set; }
        public string? PFA { get; set; }
        public string? BankName { get; set; }
        public string? AccountNumber { get; set; }
        public string? AccountType { get; set; }
    }

    // DTO for PUT/PATCH requests
    // All properties are optional, as a client may only update one field.
    public class UpdateAssetDto
    {
        public string? AssetType { get; set; }
        public string? Name { get; set; }
        public decimal? Value { get; set; }
        public string? RSAPin { get; set; }
        public string? PFA { get; set; }
        public string? BankName { get; set; }
        public string? AccountNumber { get; set; }
        public string? AccountType { get; set; }
    }
}