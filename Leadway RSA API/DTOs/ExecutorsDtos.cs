using Leadway_RSA_API.CustomValidators;
using System.ComponentModel.DataAnnotations;

namespace Leadway_RSA_API.DTOs
{
    public class CreateExecutorsDtos
    {
        [Required]
        public string ExecutorType { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? CompanyName { get; set; }
        [Required]
        public string PhoneNumber { get; set; }
        [Required]
        public string Address { get; set; }
        [Required]
        public string City { get; set; }
        [Required]
        public string State { get; set; }
    }

    public class ExecutorsDtos
    {
        public int Id { get; set; }
        public int ApplicantId { get; set; }
        public string ExecutorType { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? CompanyName { get; set; }
        public string PhoneNumber { get; set; }
        public string Address { get; set; }
        public string City { get; set; }
        public string State { get; set; }
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
