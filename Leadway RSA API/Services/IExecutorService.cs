using Leadway_RSA_API.Models;
using Leadway_RSA_API.DTOs;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Leadway_RSA_API.Services
{
    public interface IExecutorService
    {
        // New method to handle the creation of the default "Leadway Trustees" executor
        Task AddDefaultExecutorAsync(int applicantId);
        Task<Executor?> AddExecutorAsync(int applicantId, CreateExecutorsDtos executorDto);
        Task<List<Executor>> GetExecutorByApplicantIdAsync(int applicantId);
        Task<Executor?> GetExecutorAsync(int id);
        Task<Executor?> UpdateExecutorAsync(int applicantId, int id, UpdateExecutorsDtos executorDto);
        Task<bool> DeleteExecutorAsync(int applicantId, int id);
    }
}
