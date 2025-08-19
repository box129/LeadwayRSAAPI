using Leadway_RSA_API.Data;
using Leadway_RSA_API.Models;
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

        public async Task<Applicant?> CreateApplicantAsync(Applicant applicant)
        {
            // --- Business Logic & Data Assignment ---
            // Set initial audit fields (though DbContext override also handles CreatedDate)
            // Now, all the business logic from the controller's POST method is here.
            applicant.CreatedDate = DateTime.UtcNow;
            applicant.LastModifiedDate = DateTime.UtcNow;
            applicant.CurrentStep = 1; // Explicitly set current step for a new applicant
            applicant.IsComplete = false;

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

        public async Task<Applicant?> GetApplicantAsync(int id)
        {
            // The logic for finding an applicant is now encapsulated here.
            // Find the applicant by Id. Use FindAsync for primary key lookups, or FirstOrDefaultAsync.
            return await _context.Applicants.FindAsync(id);
        }

        public async Task<Applicant?> UpdateApplicantAsync(int id, Applicant applicant)
        {
            // This entire update logic is now in the service layer

            // 3. Find the existing applicant in the database first.
            // This ensures EF Core is tracking the actual entity you want to modify..
            var existingApplicant = await _context.Applicants.FindAsync(id);

            if (existingApplicant == null)
            {
                return null;
            }

            // Update the properties of the tracked entity.
            // 4. Update the properties of the *existing tracked entity* with the new values
            // from the 'applicant' object received in the request body.
            // CurrentValues.SetValues() is an efficient way to copy all scalar/complex properties.
            _context.Entry(existingApplicant).CurrentValues.SetValues(applicant);

            // Ensure LastModifiedDate is updated, as the record has changed.
            existingApplicant.LastModifiedDate = DateTime.UtcNow;

            // Ensure CreatedDate is not modified.
            // Note: If you have properties like 'CreatedDate' that should *never* be updated
            // after initial creation, you might explicitly mark them as not modified:
            _context.Entry(existingApplicant).Property(a => a.CreatedDate).IsModified = false;

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

        public async Task<bool> DeleteApplicantAsync(int id)
        {
            // 1. Find the Applicant to delete
            var applicant = await _context.Applicants.FindAsync(id);
            if (applicant == null)
            {
                return false;
            }

            // 2. Remove the Applicant
            _context.Applicants.Remove(applicant);
            await _context.SaveChangesAsync();

            // Important: Due to how you've set up relationships (one-to-many, e.g., Applicant has ICollection<Beneficiary>),
            // Entity Framework Core will, by default, handle cascading deletes.
            // This means when you delete an Applicant, all its associated Identifications, Beneficiaries, Assets,
            // Executors, Guardians, BeneficiaryAssetAllocations, and PaymentTransactions will also be deleted from the database.
            // This is generally desired for parent-child relationships like this, but be aware of its impact.
            return true;
        }

        private bool ApplicantExists(int id)
        {
            return _context.Applicants.Any(e => e.Id == id);
        }
    }
}
