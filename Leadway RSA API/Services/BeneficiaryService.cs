using Leadway_RSA_API.Data;
using Leadway_RSA_API.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Leadway_RSA_API.Services
{
    public class BeneficiaryService : IBeneficiaryService
    {
        private readonly ApplicationDbContext _context;
        // In a real application, you might inject a logger as well
        // private readonly ILogger<ApplicantService> _logger;

        public BeneficiaryService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Beneficiary?> AddBeneficiaryAsync(int applicantId, Beneficiary beneficiary)
        {
            // Check if the applicant exists
            var applicant = await _context.Applicants.FindAsync(applicantId);
            if (applicant == null)
            {
                return null; // Signals to the controller that the applicant was not found.
            }

            // Associate Beneficiary with Applicant and set default values
            beneficiary.ApplicantId = applicantId; // Ensure the FK is correctly set

            // We are only creating the Beneficiary itself in this POST request.
            // Asset allocations are handled separately.
            beneficiary.AssetAllocations = new List<BeneficiaryAssetAllocation>();

            _context.Beneficiaries.Add(beneficiary);
            await _context.SaveChangesAsync();

            // Optional: Update the Applicant's LastModifiedDate as well
            applicant.LastModifiedDate = DateTime.UtcNow;
            await _context.SaveChangesAsync(); // Save the update to the applicant

            return beneficiary;
        }

        public async Task<List<Beneficiary>> GetBeneficiariesByApplicantIdAsync(int applicantId)
        {
            // Retrieve beneficiaries for the given applicant. Use ToListAsync() to execute the query.
            var beneficiaries = await _context.Beneficiaries
                .Where(b => b.ApplicantId == applicantId)
                .ToListAsync();

            // --- ADD THIS BLOCK ---
            // If lazy loading is enabled, the Applicant property will be populated.
            // Explicitly set it to null before returning to prevent serialization cycles.
            foreach (var beneficiary in beneficiaries)
            {
                beneficiary.Applicant = null;
            }
            // --- END OF ADDED BLOCK ---

            return beneficiaries;
        }

        public async Task<Beneficiary?> GetBeneficiaryAsync(int id)
        {
            // FindAsync is sufficient as it only needs the primary key.
            return await _context.Beneficiaries.FindAsync(id);
        }

        public async Task<Beneficiary?> UpdateBeneficiaryAsync(int applicantId, int id, Beneficiary beneficiary)
        {
            // The service validates that the IDs match.
            if (id != beneficiary.Id || applicantId != beneficiary.ApplicantId)
            {
                return null; // Signals a bad request.
            }

            var existingBeneficiary = await _context.Beneficiaries.FirstOrDefaultAsync(b => b.Id == id && b.ApplicantId == applicantId);
            if (existingBeneficiary == null)
            {
                return null; // Beneficiary not found or doesn't belong to the applicant.
            }

            _context.Entry(existingBeneficiary).CurrentValues.SetValues(beneficiary);

            try
            {
                await _context.SaveChangesAsync();

                var applicant = await _context.Applicants.FindAsync(applicantId);
                if (applicant != null)
                {
                    applicant.LastModifiedDate = DateTime.UtcNow;
                    await _context.SaveChangesAsync();
                }
                return existingBeneficiary;
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!BeneficiaryExists(id, applicantId))
                {
                    return null;
                }
                else
                {
                    throw;
                }
            }
        }

        public async Task<bool> DeleteBeneficiaryAsync(int applicantId, int id)
        {
            var beneficiary = await _context.Beneficiaries.FirstOrDefaultAsync(b => b.Id == id && b.ApplicantId == applicantId);

            if (beneficiary == null)
            {
                return false; // Signals to the controller that the record was not found.
            }

            _context.Beneficiaries.Remove(beneficiary);
            await _context.SaveChangesAsync();

            var applicant = await _context.Applicants.FindAsync(applicantId);
            if (applicant != null)
            {
                applicant.LastModifiedDate = DateTime.UtcNow;
                await _context.SaveChangesAsync();
            }

            return true;
        }

        public bool BeneficiaryExists(int id, int applicantId)
        {
            return _context.Beneficiaries.Any(e => e.Id == id && e.ApplicantId == applicantId);
        }
    }
}
