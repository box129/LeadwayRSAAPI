using Leadway_RSA_API.Data;
using Leadway_RSA_API.Models;
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

        public async Task<Identification?> AddIdentificationAsync(int applicantId, Identification identification)
        {
            // 2. Check if the Applicant exists
            var applicant = await _context.Applicants.FindAsync(applicantId);
            if (applicant == null)
            {
                return null; // Signals to the controller that the applicant was not found.
            }

            // 3. Associate Identification with Applicant and set default values
            identification.ApplicantId = applicantId; // Ensure the FK is correctly set
            identification.UploadDate = DateTime.UtcNow; // Set upload date on the server

            _context.Identifications.Add(identification);
            await _context.SaveChangesAsync();

            // Update the parent Applicant's LastModifiedDate here, where the business logic belongs.
            applicant.LastModifiedDate = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return identification;
        }

        public async Task<List<Identification>> GetIdentificationsByApplicantIdAsync(int applicantId)
        {
            var identifications = await _context.Identifications
                .Where(i => i.ApplicantId == applicantId)
                .ToListAsync();


            // --- ADD THIS BLOCK ---
            // If lazy loading is enabled, the Applicant property will be populated.
            // Explicitly set it to null before returning to prevent serialization cycles.
            // Perform circular reference cleanup in the service before returning the data.
            foreach (var id in identifications)
            {
                id.Applicant = null;
            }

            return identifications;
        }

        public async Task<Identification?> GetIdentificationAsync(int id)
        {
            // Find the identification by its primary key
            return await _context.Identifications.FindAsync(id);
        }

        public async Task<Identification?> UpdateIdentificationAsync(int applicantId, int id, Identification identification)
        {
            // The service ensures the IDs in the route match the IDs in the body.
            if (id != identification.Id || applicantId != identification.ApplicantId)
            {
                return null; // Return null if IDs don't match, signaling a Bad Request.
            }

            // Securely find the existing Identification record using BOTH the identification's ID
            // AND the parent applicant's ID to ensure it belongs to the correct applicant.
            var existingIdentification = await _context.Identifications
                .FirstOrDefaultAsync(i => i.Id == id && i.ApplicantId == applicantId);

            if (existingIdentification == null)
            {
                return null; // Signals to the controller that the record was not found.
            }

            // Update the properties of the tracked entity with the new values.
            _context.Entry(existingIdentification).CurrentValues.SetValues(identification);

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

        private bool IdentificationExists(int id, int applicantId)
        {
            return _context.Identifications.Any(e => e.Id == id && e.ApplicantId == applicantId);
        }

    }
}
