using Leadway_RSA_API.Models;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace Leadway_RSA_API.Services
{
    public interface IIdentificationService
    {
        Task<Identification?> AddIdentificationAsync(int applicantId, Identification identification);
        Task<List<Identification>> GetIdentificationsByApplicantIdAsync(int applicantId);
        Task<Identification?> GetIdentificationAsync(int id);
        Task<Identification?> UpdateIdentificationAsync(int applicantId, int id, Identification identification);
        Task<bool> DeleteIdentificationAsync(int applicantId, int id);
    }
}
