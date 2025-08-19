using Leadway_RSA_API.Models;
using System.Collections.Generic;
using System.Threading.Tasks;


namespace Leadway_RSA_API.Services
{
    public interface IAssetService
    {
        Task<Asset?> AddAssetAsync(int applicantId, Asset asset);
        Task<List<Asset>> GetAssetsByApplicantIdAsync(int applicantId);
        Task<Asset?> GetAssetAsync(int id);
        Task<Asset?> UpdateAssetAsync(int applicantId, int id, Asset asset);
        Task<bool> DeleteAssetAsync(int applicantId, int id);
    }
}
