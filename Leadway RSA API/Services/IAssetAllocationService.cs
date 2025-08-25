using Leadway_RSA_API.Models;
using Leadway_RSA_API.DTOs;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Leadway_RSA_API.Services
{
    public interface IAssetAllocationService
    {
        Task<BeneficiaryAssetAllocation?> AddAssetAllocationAsync(int applicantId, CreateAssetAllocationDto allocationDto);
        Task<List<BeneficiaryAssetAllocation>> GetAssetAllocationsByApplicantIdAsync(int applicantId);
        Task<BeneficiaryAssetAllocation?> GetAssetAllocationByIdAsync(int id);
        Task<BeneficiaryAssetAllocation?> UpdateBeneficiaryAssetAllocationAsync(int applicantId, int id, UpdateAssetAllocationDto allocationDto);
        Task<bool> DeleteBeneficiaryAssetAllocationAsync(int applicantId, int id);
    }
}
