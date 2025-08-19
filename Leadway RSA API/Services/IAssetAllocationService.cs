using Leadway_RSA_API.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Leadway_RSA_API.Services
{
    public interface IAssetAllocationService
    {
        Task<BeneficiaryAssetAllocation?> AddAssetAllocationAsync(int applicantId, BeneficiaryAssetAllocation allocation);
        Task<List<BeneficiaryAssetAllocation>> GetAssetAllocationsByApplicantIdAsync(int applicantId);
        Task<BeneficiaryAssetAllocation?> GetAssetAllocationAsync(int id);
        Task<BeneficiaryAssetAllocation?> UpdateBeneficiaryAssetAllocationAsync(int applicantId, int id, BeneficiaryAssetAllocation allocation);
        Task<bool> DeleteBeneficiaryAssetAllocationAsync(int applicantId, int id);
    }
}
