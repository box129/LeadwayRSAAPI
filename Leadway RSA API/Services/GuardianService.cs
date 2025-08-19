using Leadway_RSA_API.Data;
using Leadway_RSA_API.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System;

namespace Leadway_RSA_API.Services
{
    public class GuardianService : IGuardianService
    {
        private readonly ApplicationDbContext _context;

        public GuardianService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Guardian?> AddGuardianAsync(int applicantId, Guardian guardian)
        {
            // Check if the Applicant exists
            var applicant = await _context.Applicants.FindAsync(applicantId);
            if (applicant == null)
            {
                return null;
                //return NotFound($"Applicant with ID {applicantId} not found.");
            }

            // 3. Associate Guardian with Applicant
            guardian.ApplicantId = applicantId; // Enssure the foreign is correctly set

            // Important: CLear any nested navigation property for Applicnat in the incoming object
            // We're linking to an existing Applicant, not creating a new one.
            guardian.Applicant = null;

            _context.Guardians.Add(guardian);
            await _context.SaveChangesAsync();

            // Optional: Update the Applicant's LastModifiedDate as well
            applicant.LastModifiedDate = DateTime.UtcNow;
            await _context.SaveChangesAsync(); // Save the update to the applicant

            return guardian;
        }

        public async Task<List<Guardian>> GetGuardiansByApplicantIdAsync(int applicantId)
        {
            // Correction: Return an empty list instead of null if no guardians are found.
            // This is a more robust and predictable pattern for an endpoint that returns a collection.
            var guardians = await _context.Guardians
                .Where(g => g.ApplicantId == applicantId)
                .ToListAsync();

            // --- ADD THIS BLOCK ---
            // If lazy loading is enabled, the Applicant property will be populated.
            // We explicitly set it to null to prevent serialization cycles.
            foreach (var guardian in guardians)
            {
                guardian.Applicant = null;
            }
            // --- END OF ADDED BLOCK ---

            return guardians;
        }

        public async Task<Guardian?> GetGuardianAsync(int id)
        {
            // Find the guardian by its primary key
            var guardian = await _context.Guardians
                .Include(g => g.Applicant)
                .FirstOrDefaultAsync(g => g.Id == id);

            if (guardian == null)
            {
                return null;
                //return NotFound($"Guardian with ID {id} not found.");
            }

            // Clean up the circular reference before returning.
            if (guardian.Applicant != null)
            {
                // The Applicant model might have a collection of Guardians.
                // Setting it to null prevents a serialization loop.
                guardian.Applicant.Guardians = null;
            }

            return guardian;
        }

        public async Task<Guardian?> UpdateGuardianAsync(int applicantId, int id, Guardian guardian)
        {
            if (id != guardian.Id || applicantId != guardian.ApplicantId)
            {
                return null;
            }

            var existingGuardian = await _context.Guardians
                                              .FirstOrDefaultAsync(g => g.Id == id && g.ApplicantId == applicantId);

            if (existingGuardian == null)
            {
                return null;
                //return NotFound($"Guardian with ID {id} not found or does not belong to Applicant ID {applicantId}.");
            }
            // Important: Clear the navigation property to prevent EF from trying to attach a new Applicant.
            guardian.Applicant = null;

            _context.Entry(existingGuardian).CurrentValues.SetValues(guardian);

            try
            {
                await _context.SaveChangesAsync();
                var applicant = await _context.Applicants.FindAsync(applicantId);
                if (applicant != null)
                {
                    applicant.LastModifiedDate = DateTime.UtcNow;
                    await _context.SaveChangesAsync();
                }
                return existingGuardian;
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!GuardianExists(id, applicantId))
                {
                    return null;
                    //return NotFound($"Guardian with ID {id} not found or does not belong to Applicant ID {applicantId}.");
                }
                else
                {
                    throw;
                }
            }
        }

        public async Task<bool> DeleteGuardianAsync(int applicantId, int id)
        {
            var guardian = await _context.Guardians
                                      .FirstOrDefaultAsync(g => g.Id == id && g.ApplicantId == applicantId);

            if (guardian == null)
            {
                return false;
                //return NotFound($"Guardian with ID {id} not found or does not belong to Applicant ID {applicantId}.");
            }

            _context.Guardians.Remove(guardian);
            await _context.SaveChangesAsync();

            var applicant = await _context.Applicants.FindAsync(applicantId);
            if (applicant != null)
            {
                applicant.LastModifiedDate = DateTime.UtcNow;
                await _context.SaveChangesAsync();
            }

            return true;
        }


        // Helper method
        private bool GuardianExists(int id, int applicantId)
        {
            return _context.Guardians.Any(e => e.Id == id && e.ApplicantId == applicantId);
        }


    }
}
