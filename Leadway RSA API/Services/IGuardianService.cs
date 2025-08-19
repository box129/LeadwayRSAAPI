using Leadway_RSA_API.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Leadway_RSA_API.Services
{
    public interface IGuardianService
    {
        Task<Guardian?> AddGuardianAsync(int applicantId, Guardian guardian);
        Task<List<Guardian>> GetGuardiansByApplicantIdAsync(int applicantId);
        Task<Guardian?> GetGuardianAsync(int id);
        Task<Guardian?> UpdateGuardianAsync(int applicantId, int id, Guardian guardian);
        Task<bool> DeleteGuardianAsync(int applicantId, int id);
    }
}
