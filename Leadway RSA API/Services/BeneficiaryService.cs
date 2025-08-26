using Leadway_RSA_API.Data;
using Leadway_RSA_API.Models;
using Leadway_RSA_API.DTOs;
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

        /// <summary>
        /// Adds a new Beneficiary record for a specific Applicant.
        /// The service handles mapping the DTO to the model and all business logic.
        /// </summary>
        public async Task<Beneficiary?> AddBeneficiaryAsync(int applicantId, CreateBeneficiaryDto beneficiaryDto)
        {
            // Check if the applicant exists
            var applicant = await _context.Applicants.FindAsync(applicantId);
            if (applicant == null)
            {
                return null; // Signals to the controller that the applicant was not found.
            }

            var beneficiary = new Beneficiary()
            {
                ApplicantId = applicantId,
                FirstName = beneficiaryDto.FirstName,
                LastName = beneficiaryDto.LastName,
                Gender = beneficiaryDto.Gender
            };

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

        /// <summary>
        /// Retrieves all Beneficiary records for a specific Applicant.
        /// </summary>
        public async Task<List<Beneficiary>> GetBeneficiariesByApplicantIdAsync(int applicantId)
        {
            // Retrieve beneficiaries for the given applicant. Use ToListAsync() to execute the query.
            var beneficiaries = await _context.Beneficiaries
                .Where(b => b.ApplicantId == applicantId)
                .ToListAsync();

            return beneficiaries;
        }

        /// <summary>
        /// Retrieves a single Beneficiary record by its own Id.
        /// </summary>
        public async Task<Beneficiary?> GetBeneficiaryAsync(int id)
        {
            // FindAsync is sufficient as it only needs the primary key.
            return await _context.Beneficiaries.FindAsync(id);
        }

        /// <summary>
        /// Updates an existing Beneficiary record.
        /// The service finds the model and applies updates from the DTO.
        /// </summary>
        public async Task<Beneficiary?> UpdateBeneficiaryAsync(int applicantId, int id, UpdateBeneficiaryDto beneficiaryDto)
        {
            var existingBeneficiary = await _context.Beneficiaries.FirstOrDefaultAsync(b => b.Id == id && b.ApplicantId == applicantId);
            if (existingBeneficiary == null)
            {
                return null; // Beneficiary not found or doesn't belong to the applicant.
            }

            // Apply the updates from the DTO to the existing model.
            // Only update properties that have values in the DTO.
            if (beneficiaryDto.FirstName != null)
            {
                existingBeneficiary.FirstName = beneficiaryDto.FirstName;
            }
            if (beneficiaryDto.LastName != null)
            {
                existingBeneficiary.LastName = beneficiaryDto.LastName;
            }
            if (beneficiaryDto.Gender != null)
            {
                existingBeneficiary.Gender = beneficiaryDto.Gender;
            }

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

        /// <summary>
        /// Deletes a Beneficiary record for a specific Applicant.
        /// </summary>
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

        /// <summary>
        /// A helper method to check if a beneficiary exists by ID and Applicant ID.
        /// </summary>
        public bool BeneficiaryExists(int id, int applicantId)
        {
            return _context.Beneficiaries.Any(e => e.Id == id && e.ApplicantId == applicantId);
        }
    }
}
