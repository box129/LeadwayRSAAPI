using Leadway_RSA_API.Data;
using Leadway_RSA_API.Models;
using Leadway_RSA_API.DTOs;
using Microsoft.EntityFrameworkCore;

namespace Leadway_RSA_API.Services
{
    public class IdentificationService : IIdentificationService
    {
        private readonly ApplicationDbContext _context;

        public IdentificationService(ApplicationDbContext context)
        {
            _context = context;
        }


        /// <summary>
        /// Adds a new Identification record for a specific Applicant.
        /// The service handles mapping the DTO to the model and all business logic.
        /// </summary>
        public async Task<Identification?> AddIdentificationAsync(int applicantId, CreateIdentificationDto identificationDto)
        {
            // Check if the Applicant exists
            var applicant = await _context.Applicants.FindAsync(applicantId);
            if (applicant == null)
            {
                return null; // Signals to the controller that the applicant was not found.
            }

            // The service is responsible for mapping the DTO to the model.
            var identification = new Identification
            {
                ApplicantId = applicantId,
                IdentificationType = identificationDto.IdentificationType,
                DocumentNumber = identificationDto.DocumentNumber,
                ImagePath = identificationDto.ImagePath,
                UploadDate = DateTime.UtcNow // Set upload date on the server.
            };

            _context.Identifications.Add(identification);
            await _context.SaveChangesAsync();

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
        /// Updates an existing Identification record.
        /// The service finds the model and applies updates from the DTO.
        /// </summary>
        public async Task<Identification?> UpdateIdentificationAsync(int applicantId, int id, UpdateIdentificationDto identificationDto)
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

            if (identificationDto.IdentificationType != null)
            {
                existingIdentification.IdentificationType = identificationDto.IdentificationType;
            }

            if (identificationDto.ImagePath != null)
            {
                existingIdentification.ImagePath = identificationDto.ImagePath;
            }

            try
            {
                // 5. Save changes to the database.
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
                    // return NotFound($"Identification with ID {id} not found or does not belong to Applicant ID {applicantId}.");
                }
                else
                {
                    throw;
                }
            }
        }

        /// <summary>
        /// Deletes an Identification record for a specific Applicant.
        /// </summary>
        public async Task<bool> DeleteIdentificationAsync(int id, int applicantId)
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

        /// <summary>
        /// A helper method to check if an identification exists by ID and Applicant ID.
        /// </summary>
        private bool IdentificationExists(int id, int applicantId)
        {
            return _context.Identifications.Any(e => e.Id == id && e.ApplicantId == applicantId);
        }

    }
}
