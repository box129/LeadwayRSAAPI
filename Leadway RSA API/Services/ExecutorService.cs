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
    public class ExecutorService : IExecutorService
    {
        private readonly ApplicationDbContext _context;

        public ExecutorService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Executor?> AddExecutorAsync(int applicantId, CreateExecutorsDtos executorDto)
        {
            // 2. Check if the Applicant Exist
            var applicant = await _context.Applicants.FindAsync(applicantId);
            if (applicant == null)
            {
                return null;
                //return NotFound($"Applicant with ID {applicantId} not found."); // Returns 404
            }

            var executor = new Executor()
            {
                ApplicantId = applicantId,
                ExecutorType = executorDto.ExecutorType,
                FirstName = executorDto.FirstName,
                LastName = executorDto.LastName,
                CompanyName = executorDto.CompanyName,
                PhoneNumber = executorDto.PhoneNumber,
                Address = executorDto.Address,
                City = executorDto.City,
                State = executorDto.State
            };

            
            _context.Executors.Add(executor);
            await _context.SaveChangesAsync();

            // Optional: Update the Applicant's LastModifiedDate as well
            applicant.LastModifiedDate = DateTime.UtcNow;
            await _context.SaveChangesAsync(); // Save the update to the applicants

            return executor;
        }

        public async Task<List<Executor>> GetExecutorByApplicantIdAsync(int applicantId)
        {
            // Retrieve executors for the given applicant. Use ToListAsync() to execute the query.
            var executors = await _context.Executors
                .Where(e => e.ApplicantId == applicantId)
                .ToListAsync(); // Executes the query

            return executors;
        }

        public async Task<Executor?> GetExecutorAsync(int id)
        {
            // Find the executor by its primary key
            var executor = await _context.Executors
                .Include(e => e.Applicant)
                .FirstOrDefaultAsync(e => e.Id == id);

            return executor;
        }

        public async Task<Executor?> UpdateExecutorAsync(int applicantId, int id, UpdateExecutorsDtos executorDto)
        {
            // 3. Check for the existing Executor in the database, if not return 404
            var existingExecutor = await _context.Executors.FirstOrDefaultAsync(e => e.Id == id && e.ApplicantId == applicantId);
            if (existingExecutor == null)
            {
                return null;
                // return NotFound($"Executor with ID {id} not found or does nor belong to Applicant ID {applicantId}.");
            }

            // CORRECTED: Apply updates only if the DTO property is not null
            if (executorDto.ExecutorType != null)
            {
                existingExecutor.ExecutorType = executorDto.ExecutorType;
            }
            if (executorDto.FirstName != null)
            {
                existingExecutor.FirstName = executorDto.FirstName;
            }
            if (executorDto.LastName != null)
            {
                existingExecutor.LastName = executorDto.LastName;
            }
            if (executorDto.CompanyName != null)
            {
                existingExecutor.CompanyName = executorDto.CompanyName;
            }
            if (executorDto.PhoneNumber != null)
            {
                existingExecutor.PhoneNumber = executorDto.PhoneNumber;
            }
            if (executorDto.Address != null)
            {
                existingExecutor.Address = executorDto.Address;
            }
            if (executorDto.City != null)
            {
                existingExecutor.City = executorDto.City;
            }
            if (executorDto.State != null)
            {
                existingExecutor.State = executorDto.State;
            }

            try
            {
                // 5. Save changes to database andnupdate the last modified date
                await _context.SaveChangesAsync();
                var applicant = await _context.Applicants.FindAsync(applicantId);
                if (applicant != null)
                {
                    applicant.LastModifiedDate = DateTime.UtcNow;
                    await _context.SaveChangesAsync();
                }
                return existingExecutor;
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ExecutorExists(id, applicantId)) // Helper method
                {
                    return null;
                    //return NotFound($"Executor with ID {id} not found or does not belong to Applicant ID {applicantId}.");
                }
                else
                {
                    throw;
                }
            }
        }

        public async Task<bool> DeleteExecutorAsync(int applicantId, int id)
        {
            var executor = await _context.Executors
                                      .FirstOrDefaultAsync(e => e.Id == id && e.ApplicantId == applicantId);

            if (executor == null)
            {
                return false;
                //return NotFound($"Executor with ID {id} not found or does not belong to Applicant ID {applicantId}.");
            }

            _context.Executors.Remove(executor);
            await _context.SaveChangesAsync();

            var applicant = await _context.Applicants.FindAsync(applicantId);
            if (applicant != null)
            {
                applicant.LastModifiedDate = DateTime.UtcNow;
                await _context.SaveChangesAsync();
            }

            return true;
        }


        // Helper method (add this inside your ApplicantsController class)
        private bool ExecutorExists(int id, int applicantId)
        {
            return _context.Executors.Any(e => e.Id == id && e.ApplicantId == applicantId);
        }
    }
}
