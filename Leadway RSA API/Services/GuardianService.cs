using Leadway_RSA_API.Data;
using Leadway_RSA_API.Models;
using Leadway_RSA_API.DTOs;
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

        public async Task<Guardian?> AddGuardianAsync(int applicantId, CreateGuardiansDtos guardianDto)
        {
            // Check if the Applicant exists
            var applicant = await _context.Applicants.FindAsync(applicantId);
            if (applicant == null)
            {
                return null;
                //return NotFound($"Applicant with ID {applicantId} not found.");
            }

            var guardian = new Guardian()
            {
               ApplicantId = applicantId,
               FirstName = guardianDto.FirstName,
               LastName = guardianDto.LastName,
               PhoneNumber = guardianDto.PhoneNumber,
               Address = guardianDto.Address,
               City = guardianDto.City,
               State = guardianDto.State
            };
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

            return guardians;
        }

        public async Task<Guardian?> GetGuardianAsync(int id)
        {
            // Find the guardian by its primary key
            var guardian = await _context.Guardians
                .Include(g => g.Applicant)
                .FirstOrDefaultAsync(g => g.Id == id);

            return guardian;
        }

        public async Task<Guardian?> UpdateGuardianAsync(int applicantId, int id, UpdateGuardiansDtos guardianDto)
        {
            var existingGuardian = await _context.Guardians
                                              .FirstOrDefaultAsync(g => g.Id == id && g.ApplicantId == applicantId);

            if (existingGuardian == null)
            {
                return null;
                //return NotFound($"Guardian with ID {id} not found or does not belong to Applicant ID {applicantId}.");
            }
            
            if (guardianDto.FirstName != null)
            {
                existingGuardian.FirstName = guardianDto.FirstName;
            }
            if (guardianDto.LastName != null)
            {
                existingGuardian.LastName = guardianDto.LastName;
            }
            if (guardianDto.PhoneNumber != null)
            {
                existingGuardian.PhoneNumber = guardianDto.PhoneNumber;
            }
            if (guardianDto.Address != null)
            {
                existingGuardian.Address = guardianDto.Address;
            }
            if (guardianDto.City != null)
            {
                existingGuardian.City = guardianDto.City;
            }
            if (guardianDto.State != null)
            {
                existingGuardian.State = guardianDto.State;
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
