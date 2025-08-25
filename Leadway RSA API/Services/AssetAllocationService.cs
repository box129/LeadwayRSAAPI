using Leadway_RSA_API.Data;
using Leadway_RSA_API.Models;
using Leadway_RSA_API.DTOs;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

namespace Leadway_RSA_API.Services
{
    public class AssetAllocationService : IAssetAllocationService
    {
        private readonly ApplicationDbContext _context;

        public AssetAllocationService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<BeneficiaryAssetAllocation?> AddAssetAllocationAsync(int applicantId, CreateAssetAllocationDto allocationDto)
        {
            // Verify if applicant exists
            var applicant = await _context.Applicants.FindAsync(applicantId);
            if (applicant == null)
            {
                return null; // Applicant not found
            }

            // Verify Asset esists AND belongs to this aplicant
            var asset = await _context.Assets
                .Where(a => a.Id == allocationDto.AssetId && a.ApplicantId == applicantId)
                .FirstOrDefaultAsync();

            if (asset == null)
            {
                return null; // Asset not found or does not belong to the applicant
            }

            // Verify Beneficiary exusts AND belongs to this Applicant
            // Use .Where() clause to ensure ownership
            var beneficiary = await _context.Beneficiaries
                .Where(b => b.Id == allocationDto.BeneficiaryId && b.ApplicantId == applicantId)
                .FirstOrDefaultAsync();
            if (beneficiary == null)
            {
                return null; // Beneficiary not found
            }

            // Check for duplicate allocation (same asset to same beneficiary for the same applicant)
            // To prevent creating identical allocation records.
            var existingAllocation = await _context.BeneficiaryAssetAllocations
                .Where(ba => ba.ApplicantId == applicantId &&
                            ba.AssetId == allocationDto.AssetId &&
                            ba.BeneficiaryId == allocationDto.BeneficiaryId)
                .FirstOrDefaultAsync();
            if (existingAllocation != null)
            {
                return null; // Duplicate allocation found
            }

            var allocation = new BeneficiaryAssetAllocation
            {
                ApplicantId = applicantId,
                AssetId = allocationDto.AssetId,
                BeneficiaryId = allocationDto.BeneficiaryId,
                Percentage = allocationDto.Percentage
            };

            _context.BeneficiaryAssetAllocations.Add(allocation);
            await _context.SaveChangesAsync();

            // Optional: Update the Applicant's LastModifiedDate
            applicant.LastModifiedDate = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return allocation;
        }

        public async Task<List<BeneficiaryAssetAllocation>> GetAssetAllocationsByApplicantIdAsync(int applicantId)
        {
            // Eager Loading
            var allocations = await _context.BeneficiaryAssetAllocations
                                            .Where(ba => ba.ApplicantId == applicantId)
                                            .Include(ba => ba.Asset)
                                            .Include(ba => ba.Beneficiary)
                                            .ToListAsync();

            return allocations;
        }

        public async Task<BeneficiaryAssetAllocation?> GetAssetAllocationByIdAsync(int id)
        {
            var allocation = await _context.BeneficiaryAssetAllocations
                .Include(ba => ba.Asset)
                .Include(ba => ba.Beneficiary)
                //.Include(ba => ba.Applicant)
                .FirstOrDefaultAsync(ba => ba.Id == id);
            if (allocation == null)
            {
                return null; // Allocation not found
            }
            
            return allocation;
        }

        public async Task<BeneficiaryAssetAllocation?> UpdateBeneficiaryAssetAllocationAsync(int applicantId, int id, UpdateAssetAllocationDto allocationDto)
        {
            var existingAllocation = await _context.BeneficiaryAssetAllocations
                                              .FirstOrDefaultAsync(a => a.Id == id && a.ApplicantId == applicantId);
            if (existingAllocation == null)
            {
                return null; // Allocation not found
            }
            // CORRECTED: Apply updates from the DTO only if the value is provided.
            if (allocationDto.AssetId.HasValue) existingAllocation.AssetId = allocationDto.AssetId.Value;
            if (allocationDto.BeneficiaryId.HasValue) existingAllocation.BeneficiaryId = allocationDto.BeneficiaryId.Value;
            if (allocationDto.Percentage.HasValue) existingAllocation.Percentage = allocationDto.Percentage.Value;

            try
            {
                await _context.SaveChangesAsync();
                var applicant = await _context.Applicants.FindAsync(applicantId);
                if (applicant != null)
                {
                    applicant.LastModifiedDate = DateTime.UtcNow;
                    await _context.SaveChangesAsync();
                }
                return existingAllocation; // Return the updated allocation
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!BeneficiaryAssetAllocationExists(id, applicantId))
                {
                    return null;
                    //return NotFound($"Allocation with ID {id} not found or does not belong to Applicant ID {applicantId}.");
                }
                else
                {
                    throw;
                }
            }
        }

        public async Task<bool> DeleteBeneficiaryAssetAllocationAsync(int applicantId, int id)
        {
            var allocation = await _context.BeneficiaryAssetAllocations
                                      .FirstOrDefaultAsync(a => a.Id == id && a.ApplicantId == applicantId);

            if (allocation == null)
            {
                return false;
                //return NotFound($"Allocation with ID {id} not found or does not belong to Applicant ID {applicantId}.");
            }

            _context.BeneficiaryAssetAllocations.Remove(allocation);
            await _context.SaveChangesAsync();

            var applicant = await _context.Applicants.FindAsync(applicantId);
            if (applicant != null)
            {
                applicant.LastModifiedDate = DateTime.UtcNow;
                await _context.SaveChangesAsync();
            }

            return true; // to indicate successfuil deletion
        }

        // Helper method
        private bool BeneficiaryAssetAllocationExists(int id, int applicantId)
        {
            return _context.BeneficiaryAssetAllocations.Any(e => e.Id == id && e.ApplicantId == applicantId);
        }
    }
}
