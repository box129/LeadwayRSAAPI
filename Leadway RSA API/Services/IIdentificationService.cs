using Leadway_RSA_API.Models;
using Leadway_RSA_API.DTOs;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace Leadway_RSA_API.Services
{
    public interface IIdentificationService
    {
        Task<Identification?> AddIdentificationAsync(int applicantId, CreateIdentificationDto identificationDto, IFormFile file);
        Task<List<Identification>> GetIdentificationsByApplicantIdAsync(int applicantId);
        Task<Identification?> GetIdentificationAsync(int id);
        Task<Identification?> UpdateIdentificationAsync(int applicantId, int id, UpdateIdentificationDto identificationDto, IFormFile? file);
        Task<Identification?> UpdateIdentificationAsync(int applicantId, int id, UpdateIdentificationDto identificationDto);
        Task<bool> DeleteIdentificationAsync(int id, int applicantId);
    }
}
