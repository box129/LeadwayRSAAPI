using Leadway_RSA_API.Models;
using Leadway_RSA_API.DTOs;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Leadway_RSA_API.Services
{
    public interface IGuardianService
    {
        Task<Guardian?> AddGuardianAsync(int applicantId, CreateGuardiansDtos guardianDto);
        Task<List<Guardian>> GetGuardiansByApplicantIdAsync(int applicantId);
        Task<Guardian?> GetGuardianAsync(int id);
        Task<Guardian?> UpdateGuardianAsync(int applicantId, int id, UpdateGuardiansDtos guardianDto);
        Task<bool> DeleteGuardianAsync(int applicantId, int id);
    }
}
