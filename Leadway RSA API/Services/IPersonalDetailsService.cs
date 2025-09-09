using Leadway_RSA_API.DTOs;
using Leadway_RSA_API.Models;

namespace Leadway_RSA_API.Services
{
    public interface IPersonalDetailsService
    {
        Task<PersonalDetails?> GetPersonalDetailsByApplicantIdAsync(int applicantId);
        Task<PersonalDetails?> CreatePersonalDetailsAsync(int applicantId, CreatePersonalDetailsDto detailsDto);
        Task<PersonalDetails?> UpdatePersonalDetailsAsync(int applicantId, UpdatePersonalDetailsDto detailsDto);
        Task<bool> UploadPassportPhotoAsync(int applicantId, IFormFile file);
        Task<bool> UploadSignatureAsync(int applicantId, IFormFile file);
    }
}
