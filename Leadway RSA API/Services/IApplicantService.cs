using Leadway_RSA_API.Models;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace Leadway_RSA_API.Services
{
    public interface IApplicantService
    {
        Task<Applicant?> CreateApplicantAsync(Applicant applicant);
        Task<Applicant?> GetApplicantAsync(int id);
        Task<Applicant?> UpdateApplicantAsync(int id, Applicant applicant);
        Task<bool> DeleteApplicantAsync(int id);
    }
}
