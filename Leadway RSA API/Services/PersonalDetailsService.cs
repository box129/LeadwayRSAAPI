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
            _env = env; // Used for file path management
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
                // You may want to return an error or null if the details already exist.
                // This prevents creating duplicate records.
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
                City = detailsDto.City
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

        public async Task<bool> UploadPassportPhotoAsync(int applicantId, IFormFile file)
        {
            var personalDetails = await GetPersonalDetailsByApplicantIdAsync(applicantId);
            if (personalDetails == null) return false;

            // Generate a unique file name to avoid collisions
            var uniqueFileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
            var uploadsFolder = Path.Combine(_env.WebRootPath, "uploads", "passports");
            var filePath = Path.Combine(uploadsFolder, uniqueFileName);

            // Ensure the directory exists
            Directory.CreateDirectory(uploadsFolder);

            // Save the file to the server
            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(fileStream);
            }

            // Update the model with the file path
            personalDetails.PassportPhotoPath = filePath;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> UploadSignatureAsync(int applicantId, IFormFile file)
        {
            var personalDetails = await GetPersonalDetailsByApplicantIdAsync(applicantId);
            if (personalDetails == null) return false;

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
