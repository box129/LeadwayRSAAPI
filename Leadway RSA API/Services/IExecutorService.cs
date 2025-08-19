using Leadway_RSA_API.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Leadway_RSA_API.Services
{
    public interface IExecutorService
    {
        Task<Executor?> AddExecutorAsync(int applicantId, Executor executor);
        Task<List<Executor>> GetExecutorByApplicantIdAsync(int applicantId);
        Task<Executor?> GetExecutorAsync(int id);
        Task<Executor?> UpdateExecutorAsync(int applicantId, int id, Executor executor);
        Task<bool> DeleteExecutorAsync(int applicantId, int id);
    }
}
