using Leadway_RSA_API.CustomValidators;
using System.ComponentModel.DataAnnotations;

namespace Leadway_RSA_API.DTOs
{
    public class CreateExecutorsDtos
    {
        [Required]
        public required string ExecutorType { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? CompanyName { get; set; }
        [Required]
        public required string PhoneNumber { get; set; }
        [Required]
        public required string Address { get; set; }
        [Required]
        public required string City { get; set; }
        [Required]
        public required string State { get; set; }
    }

    public class ExecutorsDtos
    {
        public int Id { get; set; }
        public int ApplicantId { get; set; }
        // Added to distinguish between default and user-added executors.
        public bool IsDefault { get; set; }

        // Added to hold the name of the default executor ("Leadway Trustees").
        // It's nullable as it's not used for user-added executors.
        public string? Name { get; set; }

        // Made nullable because the default executor doesn't have an ExecutorType.
        public string? ExecutorType { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? CompanyName { get; set; }
        public required string PhoneNumber { get; set; }
        public required string Address { get; set; }
        public required string City { get; set; }
        public required string State { get; set; }
    }

    public class UpdateExecutorsDtos
    {
        public string? ExecutorType { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? CompanyName { get; set; }
        public string? PhoneNumber { get; set; }
        public string? Address { get; set; }
        public string? City { get; set; }
        public string? State { get; set; }
    }
}
