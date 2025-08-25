using Leadway_RSA_API.Models;
using Leadway_RSA_API.DTOs;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace Leadway_RSA_API.Services
{
    public interface IApplicantService
    {
        Task<Applicant?> CreateApplicantAsync(CreateApplicantDto applicantDto);
        Task<Applicant?> GetApplicantAsync(int id);
        Task<Applicant?> UpdateApplicantAsync(int id, UpdateApplicantDto applicantDto);
        Task<bool> DeleteApplicantAsync(int id);
    }
}
