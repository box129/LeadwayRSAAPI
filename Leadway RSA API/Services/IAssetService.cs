using Leadway_RSA_API.Models;
using Leadway_RSA_API.DTOs; // Important: The interface needs to know about the DTOs
using System.Collections.Generic;
using System.Threading.Tasks;


namespace Leadway_RSA_API.Services
{
    public interface IAssetService
    {
        Task<Asset> AddAssetAsync(int applicantId, CreateAssetDto assetDto);
        Task<List<Asset>> GetAssetsByApplicantIdAsync(int applicantId);
        Task<Asset> GetAssetAsync(int id);
        Task<Asset> UpdateAssetAsync(int applicantId, int id, UpdateAssetDto assetDto);
        Task<bool> DeleteAssetAsync(int applicantId, int id);
    }
}
