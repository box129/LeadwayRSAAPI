using Leadway_RSA_API.Data;
using Leadway_RSA_API.Models;
using Leadway_RSA_API.DTOs;
using Microsoft.EntityFrameworkCore;

namespace Leadway_RSA_API.Services
{
    public class IdentificationService : IIdentificationService
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _env;

        public IdentificationService(ApplicationDbContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }

        /// <summary>
        /// Adds a new Identification record, including the image, for a specific Applicant.
        /// The service handles mapping the DTO to the model, saving the image, and all business logic.
        /// </summary>
        public async Task<Identification?> AddIdentificationAsync(int applicantId, CreateIdentificationDto identificationDto, IFormFile file)
        {
            // Check if the Applicant exists
            var applicant = await _context.Applicants.FindAsync(applicantId);
            if (applicant == null)
            {
                return null; // Signals to the controller that the applicant was not found.
            }

            // If a record is found, we can't create a new one.
            var existingIdentification = await _context.Identifications
                .FirstOrDefaultAsync(i => i.ApplicantId == applicantId);

            if (existingIdentification != null)
            {
                // Return null or throw a specific exception to signal that a record already exists.
                // This makes the API's response more specific.
                return null;
            }

            if (!Enum.TryParse<IdentificationType>(identificationDto.IdentificationType, true, out var identificationType))
            {
                // Handle invalid enum string, e.g., return null or throw a custom exception
                return null;
            }

            // The service is responsible for mapping the DTO to the model.
            var identification = new Identification
            {
                ApplicantId = applicantId,
                IdentificationType = identificationType,
                DocumentNumber = identificationDto.DocumentNumber,
                UploadDate = DateTime.UtcNow // Set upload date on the server.
            };

            // First, add the identification record to the database
            _context.Identifications.Add(identification);
            await _context.SaveChangesAsync();

            // Then, save the image to the file system
            if (file != null)
            {
                await SaveIdentificationImage(identification, file);
            }

            // Update the parent Applicant's LastModifiedDate here, where the business logic belongs.
            applicant.LastModifiedDate = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return identification;
        }

        /// <summary>
        /// Retrieves all Identification records for a specific Applicant.
        /// </summary>
        public async Task<List<Identification>> GetIdentificationsByApplicantIdAsync(int applicantId)
        {
            var identifications = await _context.Identifications
                .Where(i => i.ApplicantId == applicantId)
                .ToListAsync();

            return identifications;
        }

        /// <summary>
        /// Retrieves a single Identification record by its own Id.
        /// </summary>
        public async Task<Identification?> GetIdentificationAsync(int id)
        {
            // Find the identification by its primary key
            return await _context.Identifications.FindAsync(id);
        }

        /// <summary>
        /// Updates an existing Identification record, including the image if provided.
        /// The service finds the model and applies updates from the DTO.
        /// This method is for multipart form data requests that may include a file.
        /// </summary>
        public async Task<Identification?> UpdateIdentificationAsync(int applicantId, int id, UpdateIdentificationDto identificationDto, IFormFile? file)
        {
            // Securely find the existing Identification record using BOTH the identification's ID
            // AND the parent applicant's ID to ensure it belongs to the correct applicant.
            var existingIdentification = await _context.Identifications
                .FirstOrDefaultAsync(i => i.Id == id && i.ApplicantId == applicantId);

            if (existingIdentification == null)
            {
                return null; // Signals to the controller that the record was not found.
            }

            // Apply the updates from the DTO to the existing model.
            // Only update properties that have a value in the DTO.
            if (identificationDto.DocumentNumber != null)
            {
                existingIdentification.DocumentNumber = identificationDto.DocumentNumber;
            }

            if (Enum.TryParse<IdentificationType>(identificationDto.IdentificationType, true, out var identificationType))
            {
                existingIdentification.IdentificationType = identificationType;
            }

            // Handle the optional file update
            if (file != null)
            {
                await SaveIdentificationImage(existingIdentification, file);
            }

            try
            {
                // Save changes to the database.
                await _context.SaveChangesAsync();

                // Update the parent Applicant's LastModifiedDate here.
                var applicant = await _context.Applicants.FindAsync(applicantId);
                if (applicant != null)
                {
                    applicant.LastModifiedDate = DateTime.UtcNow;
                    await _context.SaveChangesAsync();
                }

                return existingIdentification;
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!IdentificationExists(id, applicantId)) // Helper to check if it exists and belongs to applicant
                {
                    return null;
                }
                else
                {
                    throw;
                }
            }
        }

        /// <summary>
        /// Updates an existing Identification record without an image file.
        /// This overload is specifically for admin requests that only modify metadata.
        /// </summary>
        public async Task<Identification?> UpdateIdentificationAsync(int applicantId, int id, UpdateIdentificationDto identificationDto)
        {
            // This method now calls the main update logic, without providing a file.
            return await this.UpdateIdentificationAsync(applicantId, id, identificationDto, file: null);
        }

        /// <summary>
        /// Deletes an Identification record for a specific Applicant.
        /// </summary>
        public async Task<bool> DeleteIdentificationAsync(int applicantId, int id)
        {
            // 1. Find the Identification to delete, ensuring it belongs to the specified Applicant.
            var identification = await _context.Identifications.FirstOrDefaultAsync(i => i.Id == id && i.ApplicantId == applicantId);
            if (identification == null)
            {
                return false;
            }

            _context.Identifications.Remove(identification);
            await _context.SaveChangesAsync();

            // Update the Applicant's LastModifiedDate from here.
            var applicant = await _context.Applicants.FindAsync(applicantId);
            if (applicant != null)
            {
                applicant.LastModifiedDate = DateTime.UtcNow;
                await _context.SaveChangesAsync();
            }

            return true;
        }

        // --- Private Helper Method ---
        private async Task<bool> SaveIdentificationImage(Identification identification, IFormFile file)
        {
            var uniqueFileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
            var uploadsFolder = Path.Combine(_env.WebRootPath, "uploads", "identifications");
            var filePath = Path.Combine(uploadsFolder, uniqueFileName);

            Directory.CreateDirectory(uploadsFolder);

            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(fileStream);
            }

            identification.ImagePath = filePath;
            await _context.SaveChangesAsync();
            return true;
        }

        /// <summary>
        /// A helper method to check if an identification exists by ID and Applicant ID.
        /// </summary>
        private bool IdentificationExists(int id, int applicantId)
        {
            return _context.Identifications.Any(e => e.Id == id && e.ApplicantId == applicantId);
        }
    }
}
