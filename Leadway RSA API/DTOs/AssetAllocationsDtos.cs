using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;

namespace Leadway_RSA_API.DTOs
{
    // DTO for incoming POST requests
    // This is the data the client sends to create a new Allocation record.
    public class CreateAssetAllocationDto
    {
        [Required]
        public int AssetId { get; set; }
        [Required]
        public int BeneficiaryId { get; set; }
        [Required]
        [Range(0, 100, ErrorMessage = "Percentage must be between 0 and 100.")]
        public decimal Percentage { get; set; }
    }

    // DTO for outgoing GET responses
    // This prevents over-fetching and circular references.
    public class AssetAllocationDto
    {
        public int Id { get; set; }
        public int ApplicantId { get; set; }
        public int AssetId { get; set; }
        public int BeneficiaryId { get; set; }
        public decimal Percentage { get; set; }

        // DTOs for nested data to prevent over-fetching
        public SimpleAssetDto? Asset { get; set; }
        public SimpleBeneficiaryDto? Beneficiary { get; set; }
    }

    // New DTO for PUT/PATCH requests.
    // All properties are optional, so the client can update just one field.
    public class UpdateAssetAllocationDto
    {
        public int? AssetId { get; set; }
        public int? BeneficiaryId { get; set; }
        [Range(0, 100)]
        public decimal? Percentage { get; set; }
    }

    // A simple DTO for the related Asset
    public class SimpleAssetDto
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public string? AssetType { get; set; }
    }

    // A simple DTO for the related Beneficiary
    public class SimpleBeneficiaryDto
    {
        public int Id { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
    }
}
