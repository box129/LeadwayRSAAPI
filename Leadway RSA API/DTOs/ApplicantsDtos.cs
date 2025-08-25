using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;

namespace Leadway_RSA_API.DTOs
{
    // DTOs/CreateApplicantDto.cs
    public class CreateApplicantDto
    {
        public string? RSAPin { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string PhoneNumber { get; set; }
        public string EmailAddress { get; set; }
        public DateTime DateOfBirth { get; set; }
    }

    // DTOs/ApplicantDto.cs
    public class ApplicantDto
    {
        public int Id { get; set; }
        public string? RSAPin { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string PhoneNumber { get; set; }
        public string EmailAddress { get; set; }
        public DateTime DateOfBirth { get; set; }
        public int CurrentStep { get; set; }
        public bool IsComplete { get; set; }
    }

    // DTO for updating an existing Applicant
    // It contains only the fields that a client is allowed to modify.
    public class UpdateApplicantDto
    {
        public string? RSAPin { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? PhoneNumber { get; set; }
        public string? EmailAddress { get; set; }
        public DateTime? DateOfBirth { get; set; }
    }

}