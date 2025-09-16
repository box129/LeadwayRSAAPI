using Leadway_RSA_API.Data;
using Leadway_RSA_API.DTOs;
using Leadway_RSA_API.Models;
using Microsoft.EntityFrameworkCore;

namespace Leadway_RSA_API.Services
{
    public class PersonalDetailsService : IPersonalDetailsService
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _env;

        public PersonalDetailsService(ApplicationDbContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }

        public async Task<PersonalDetails?> GetPersonalDetailsByApplicantIdAsync(int applicantId)
        {
            return await _context.PersonalDetails.FirstOrDefaultAsync(pd => pd.ApplicantId == applicantId);
        }

        public async Task<PersonalDetails?> CreatePersonalDetailsAsync(int applicantId, CreatePersonalDetailsDto detailsDto)
        {
            // Check if PersonalDetails already exists for the Applicant
            var existingDetails = await _context.PersonalDetails.AnyAsync(pd => pd.ApplicantId == applicantId);
            if (existingDetails)
            {
                return null;
            }

            var personalDetails = new PersonalDetails
            {
                ApplicantId = applicantId,
                PlaceOfBirth = detailsDto.PlaceOfBirth,
                Religion = detailsDto.Religion,
                Gender = detailsDto.Gender,
                HomeAddress = detailsDto.HomeAddress,
                State = detailsDto.State,
                City = detailsDto.City,
                PassportPhotoPath = null, // Ensure these are null initially
                SignaturePath = null
            };

            _context.PersonalDetails.Add(personalDetails);
            await _context.SaveChangesAsync();
            return personalDetails;
        }

        public async Task<PersonalDetails?> UpdatePersonalDetailsAsync(int applicantId, UpdatePersonalDetailsDto detailsDto)
        {
            var personalDetails = await _context.PersonalDetails.FirstOrDefaultAsync(pd => pd.ApplicantId == applicantId);
            if (personalDetails == null)
            {
                return null;
            }

            if (detailsDto.PlaceOfBirth != null) personalDetails.PlaceOfBirth = detailsDto.PlaceOfBirth;
            if (detailsDto.Religion != null) personalDetails.Religion = detailsDto.Religion;
            if (detailsDto.Gender != null) personalDetails.Gender = detailsDto.Gender;
            if (detailsDto.HomeAddress != null) personalDetails.HomeAddress = detailsDto.HomeAddress;
            if (detailsDto.State != null) personalDetails.State = detailsDto.State;
            if (detailsDto.City != null) personalDetails.City = detailsDto.City;

            await _context.SaveChangesAsync();
            return personalDetails;
        }

        /// <summary>
        /// Uploads a passport photo for the first time or updates an existing one.
        /// </summary>
        public async Task<bool> UploadOrUpdatePassportPhotoAsync(int applicantId, IFormFile file)
        {
            var personalDetails = await GetPersonalDetailsByApplicantIdAsync(applicantId);
            if (personalDetails == null)
            {
                // No personal details found to attach the photo to.
                return false;
            }

            // Check if an existing photo needs to be deleted.
            // This is the core of the "update" logic.
            if (!string.IsNullOrEmpty(personalDetails.PassportPhotoPath))
            {
                // Delete the old file if it exists.
                if (System.IO.File.Exists(personalDetails.PassportPhotoPath))
                {
                    System.IO.File.Delete(personalDetails.PassportPhotoPath);
                }
            }

            // Now, save the new file. The SavePassportPhoto method handles the file I/O and database update.
            return await SavePassportPhoto(personalDetails, file);
        }

        /// <summary>
        /// Uploads a signature for the first time or updates an existing one.
        /// </summary>
        public async Task<bool> UploadOrUpdateSignatureAsync(int applicantId, IFormFile file)
        {
            var personalDetails = await GetPersonalDetailsByApplicantIdAsync(applicantId);
            if (personalDetails == null)
            {
                // No personal details found to attach the signature to.
                return false;
            }

            // Check if an existing signature needs to be deleted.
            if (!string.IsNullOrEmpty(personalDetails.SignaturePath))
            {
                // Delete the old file if it exists.
                if (System.IO.File.Exists(personalDetails.SignaturePath))
                {
                    System.IO.File.Delete(personalDetails.SignaturePath);
                }
            }

            // Now, save the new file. The SaveSignature method handles the file I/O and database update.
            return await SaveSignature(personalDetails, file);
        }

        // --- Private Helper Methods ---

        private async Task<bool> SavePassportPhoto(PersonalDetails personalDetails, IFormFile file)
        {
            var uniqueFileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
            var uploadsFolder = Path.Combine(_env.WebRootPath, "uploads", "passports");
            var filePath = Path.Combine(uploadsFolder, uniqueFileName);

            Directory.CreateDirectory(uploadsFolder);

            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(fileStream);
            }

            personalDetails.PassportPhotoPath = filePath;
            await _context.SaveChangesAsync();
            return true;
        }

        private async Task<bool> SaveSignature(PersonalDetails personalDetails, IFormFile file)
        {
            var uniqueFileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
            var uploadsFolder = Path.Combine(_env.WebRootPath, "uploads", "signatures");
            var filePath = Path.Combine(uploadsFolder, uniqueFileName);

            Directory.CreateDirectory(uploadsFolder);

            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(fileStream);
            }

            personalDetails.SignaturePath = filePath;
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
