using Leadway_RSA_API.Data;
using Leadway_RSA_API.Models;
using Leadway_RSA_API.DTOs;
using Microsoft.EntityFrameworkCore;

namespace Leadway_RSA_API.Services
{
    public class ApplicantService : IApplicantService
    {
        private readonly ApplicationDbContext _context;
        // In a real application, you might inject a logger as well
        // private readonly ILogger<ApplicantService> _logger;

        public ApplicantService(ApplicationDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Creates a new Applicant record.
        /// The service handles mapping the DTO to the model and all business logic.
        /// </summary>
        /// <param name="applicantDto">The DTO containing the applicant's details.</param>
        /// <returns>The created Applicant model, or null if a conflict occurs.</returns>
        public async Task<Applicant?> CreateApplicantAsync(CreateApplicantDto applicantDto)
        {
            // The service is responsible for mapping the DTO to the model.
            var applicant = new Applicant
            {
                RSAPin = applicantDto.RSAPin,
                FirstName = applicantDto.FirstName,
                LastName = applicantDto.LastName,
                PhoneNumber = applicantDto.PhoneNumber,
                EmailAddress = applicantDto.EmailAddress,
                // The fix: Convert the DateOfBirth to UTC
                DateOfBirth = applicantDto.DateOfBirth.ToUniversalTime(),

                // The service also sets any initial business-related values.
                CreatedDate = DateTime.UtcNow,
                LastModifiedDate = DateTime.UtcNow,
                CurrentStep = 1,
                IsComplete = false
            };

            try
            {
                // Add the new applicant to the DbContext
                _context.Applicants.Add(applicant);
                // Save changes to the database
                await _context.SaveChangesAsync();
                return applicant;
            }
            catch (DbUpdateException ex)
            {
                // Handle duplicate key errors (e.g., email address)
                if (ex.InnerException?.Message.Contains("duplicate key value violates unique constraint") == true)
                {
                    // This error handling is now centralized in the service.
                    // The controller will receive a null and handle the conflict.
                    return null;
                }

                // You would log other errors here.
                throw;
            }
        }

        /// <summary>
        /// Retrieves a single Applicant record by ID.
        /// </summary>
        /// <param name="id">The ID of the applicant to retrieve.</param>
        /// <returns>The Applicant model, or null if not found.</returns>
        public async Task<Applicant?> GetApplicantAsync(int id)
        {
            // The logic for finding an applicant is now encapsulated here.
            // Find the applicant by Id. Use FindAsync for primary key lookups, or FirstOrDefaultAsync.
            return await _context.Applicants.FindAsync(id);
        }

        /// <summary>
        /// Updates an existing Applicant record.
        /// The service finds the model and applies updates from the DTO.
        /// </summary>
        /// <param name="id">The ID of the applicant to update.</param>
        /// <param name="applicantDto">The DTO with the updated details.</param>
        /// <returns>The updated Applicant model, or null if not found.</returns>
        public async Task<Applicant?> UpdateApplicantAsync(int id, UpdateApplicantDto applicantDto)
        {
            // Find the existing record first.
            var existingApplicant = await _context.Applicants.FindAsync(id);

            if (existingApplicant == null)
            {
                return null;
            }

            // Apply updates from the DTO. This ensures only provided values are changed.
            if (applicantDto.RSAPin != null) existingApplicant.RSAPin = applicantDto.RSAPin;
            if (applicantDto.FirstName != null) existingApplicant.FirstName = applicantDto.FirstName;
            if (applicantDto.LastName != null) existingApplicant.LastName = applicantDto.LastName;
            if (applicantDto.PhoneNumber != null) existingApplicant.PhoneNumber = applicantDto.PhoneNumber;
            if (applicantDto.EmailAddress != null) existingApplicant.EmailAddress = applicantDto.EmailAddress;
            if (applicantDto.DateOfBirth.HasValue) existingApplicant.DateOfBirth = applicantDto.DateOfBirth.Value;
            //if (applicantDto.CurrentStep.HasValue) existingApplicant.CurrentStep = applicantDto.CurrentStep.Value;
            //if (applicantDto.IsComplete.HasValue) existingApplicant.IsComplete = applicantDto.IsComplete.Value;

            // Update the audit field.
            existingApplicant.LastModifiedDate = DateTime.UtcNow;

            try
            {
                // 6. Save changes to the database. EF Core will detect the modifications to 'existingApplicant'.
                await _context.SaveChangesAsync();
                return existingApplicant;
            }
            catch (DbUpdateConcurrencyException)
            {
                // 7. Handle concurrency conflicts (if another user updated simultaneously
                if (!ApplicantExists(id)) // helper method to  check if the applicant stil exists
                {
                    return null; // Not found due to a concurrency issue
                }
                else
                {
                    throw; // Re-throw for other concurrency issues
                }
            }
        }

        /// <summary>
        /// Deletes an Applicant record.
        /// </summary>
        /// <param name="id">The ID of the applicant to delete.</param>
        /// <returns>True if the applicant was deleted, otherwise false.</returns>
        public async Task<bool> DeleteApplicantAsync(int id)
        {
            var applicant = await _context.Applicants
                .Include(a => a.AssetAllocations)
                .Include(a => a.Beneficiaries)
                .Include(a => a.Guardians)
                .Include(a => a.PaymentTransactions)
                .FirstOrDefaultAsync(a => a.Id == id);

            if (applicant == null)
            {
                return false;
            }

            // 1. Delete all associated BeneficiaryAssetAllocations
            _context.BeneficiaryAssetAllocations.RemoveRange(applicant.AssetAllocations!);

            // 2. Delete all other associated child records
            _context.Beneficiaries.RemoveRange(applicant.Beneficiaries!);
            _context.Guardians.RemoveRange(applicant.Guardians!);
            _context.PaymentTransactions.RemoveRange(applicant.PaymentTransactions!);

            // 3. Finally, delete the parent Applicant
            _context.Applicants.Remove(applicant);

            // 4. Save all changes in a single transaction
            await _context.SaveChangesAsync();

            return true;
        }

        /// <summary>
        /// A helper method to check if an applicant exists by ID.
        /// </summary>
        private bool ApplicantExists(int id)
        {
            return _context.Applicants.Any(e => e.Id == id);
        }
    }
}