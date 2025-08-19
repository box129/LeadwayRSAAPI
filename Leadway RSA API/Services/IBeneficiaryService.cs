using Leadway_RSA_API.Models;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Leadway_RSA_API.Services
{
    public interface IBeneficiaryService
    {
        Task<Beneficiary?> AddBeneficiaryAsync(int applicantId, Beneficiary beneficiary);
        Task<List<Beneficiary>> GetBeneficiariesByApplicantIdAsync(int applicantId);
        Task<Beneficiary?> GetBeneficiaryAsync(int id);
        Task<Beneficiary?> UpdateBeneficiaryAsync(int applicantId, int id, Beneficiary beneficiary);
        Task<bool> DeleteBeneficiaryAsync(int applicantId, int id);
    }
}
