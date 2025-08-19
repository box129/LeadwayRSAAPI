using Leadway_RSA_API.Data;
using Leadway_RSA_API.Models;
using Microsoft.EntityFrameworkCore;

namespace Leadway_RSA_API.Services
{
    public class AssetAllocationService : IAssetAllocationService
    {
        private readonly ApplicationDbContext _context;

        public AssetAllocationService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<BeneficiaryAssetAllocation?> AddAssetAllocationAsync(int applicantId, BeneficiaryAssetAllocation allocation)
        {
            // Verify if applicant exists
            var applicant = await _context.Applicants.FindAsync(applicantId);
            if (applicant == null)
            {
                return null; // Applicant not found
            }

            // Verify Asset esists AND belongs to this aplicant
            var asset = await _context.Assets
                .Where(a => a.Id == allocation.AssetId && a.ApplicantId == applicantId)
                .FirstOrDefaultAsync();

            if (asset == null)
            {
                return null; // Asset not found or does not belong to the applicant
            }

            // Verify Beneficiary exusts AND belongs to this Applicant
            // Use .Where() clause to ensure ownership
            var beneficiary = await _context.Beneficiaries
                .Where(b => b.Id == allocation.BeneficiaryId && b.ApplicantId == applicantId)
                .FirstOrDefaultAsync();
            if (beneficiary == null)
            {
                return null; // Beneficiary not found
            }

            // Set the ApplicantId for the allocation object from the route parameter
            allocation.ApplicantId = applicantId;

            // Check for duplicate allocation (same asset to same beneficiary for the same applicant)
            // To prevent creating identical allocation records.
            var existingAllocation = await _context.BeneficiaryAssetAllocations
                .Where(ba => ba.ApplicantId == applicantId &&
                            ba.AssetId == allocation.AssetId &&
                            ba.BeneficiaryId == allocation.BeneficiaryId)
                .FirstOrDefaultAsync();
            if (existingAllocation != null)
            {
                return null; // Duplicate allocation found
            }

            // Important: Ensure navigation properties are not set in the incoming JSON payload,
            // and clear them if they were to prevent EF Core from trying to create new related entities.
            allocation.Applicant = null; // Clear if accidentally set in input
            allocation.Asset = null;     // Clear if accidentally set in input
            allocation.Beneficiary = null; // Clear if accidentally set in input

            _context.BeneficiaryAssetAllocations.Add(allocation);
            await _context.SaveChangesAsync();

            // Optional: Update the Applicant's LastModifiedDate
            applicant.LastModifiedDate = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            // To ensure the returned object includes the full Asset and Beneficiary details,
            // we re-load them or include them (ReferenceHandler.IgnoreCycles will handle the rest).
            // The _context.Entry().Reference().LoadAsync() ensures navigation properties are loaded
            // if they weren't implicitly included by EF Core.
            await _context.Entry(allocation).Reference(a => a.Asset).LoadAsync();
            await _context.Entry(allocation).Reference(a => a.Beneficiary).LoadAsync();
            // await _context.Entry(allocation).Reference(a => a.Applicant).LoadAsync(); // Optional: if you want applicant nested

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

            //// To prevent cuircular reference issues in JSON serialization
            //foreach (var allocation in allocations)
            //{
            //    allocation.Applicant = null;
            //    // Clean up the nested Asset
            //    if (allocation.Asset != null)
            //    {
            //        allocation.Asset.AssetAllocations = null;

            //        if (allocation.Asset.Applicant != null)
            //        {
            //            allocation.Asset.Applicant.Identifications = null;
            //            allocation.Asset.Applicant.Beneficiaries = null;
            //            allocation.Asset.Applicant.Executors = null;
            //            allocation.Asset.Applicant.Guardians = null;
            //            allocation.Asset.Applicant.Assets = null;
            //            allocation.Asset.Applicant.PaymentTransactions = null;
            //            allocation.Asset.Applicant.AssetAllocations = null;
            //        }
            //    }

            //    // Clean up the nested Beneficiary object
            //    if (allocation.Beneficiary != null)
            //    {
            //        // --- NEW: Fixes the 'assetAllocations' null ---
            //        allocation.Beneficiary.AssetAllocations = null;
            //        // --- END NEW ---

            //        if (allocation.Beneficiary.Applicant != null)
            //        {
            //            allocation.Beneficiary.Applicant.Identifications = null;
            //            allocation.Beneficiary.Applicant.Beneficiaries = null;
            //            allocation.Beneficiary.Applicant.Executors = null;
            //            allocation.Beneficiary.Applicant.Guardians = null;
            //            allocation.Beneficiary.Applicant.Assets = null;
            //            allocation.Beneficiary.Applicant.PaymentTransactions = null;
            //        }
            //    }
            //}

            // A simpler way to prevent circular references in the returned JSON.
            foreach (var allocation in allocations)
            {
                allocation.Applicant = null;
                if (allocation.Asset != null)
                {
                    allocation.Asset.Applicant = null;
                    allocation.Asset.AssetAllocations = null;
                }
                if (allocation.Beneficiary != null)
                {
                    allocation.Beneficiary.Applicant = null;
                    allocation.Beneficiary.AssetAllocations = null;
                }
            }

            return allocations;
        }

        public async Task<BeneficiaryAssetAllocation?> GetAssetAllocationAsync(int id)
        {
            var allocation = await _context.BeneficiaryAssetAllocations
                .Include(ba => ba.Asset)
                .Include(ba => ba.Beneficiary)
                .Include(ba => ba.Applicant)
                .FirstOrDefaultAsync(ba => ba.Id == id);
            if (allocation == null)
            {
                return null; // Allocation not found
            }
            // Clean up the deeply nested objects to prevent serialization cycles
            // Start with the main Applicant object
            //if (allocation.Applicant != null)
            //{
            //    allocation.Applicant.Identifications = null;
            //    allocation.Applicant.Beneficiaries = null;
            //    allocation.Applicant.Executors = null;
            //    allocation.Applicant.Guardians = null;
            //    allocation.Applicant.Assets = null;
            //    allocation.Applicant.AssetAllocations = null;
            //    allocation.Applicant.PaymentTransactions = null;
            //}
            //// Clean up the Applicant object nested inside the Beneficiary
            //if (allocation.Beneficiary != null && allocation.Beneficiary.Applicant != null)
            //{
            //    // Fix the circular reference from the Beneficiary -> Applicant -> AssetAllocations
            //    allocation.Beneficiary.AssetAllocations = null;

            //    allocation.Beneficiary.Applicant.Identifications = null;
            //    allocation.Beneficiary.Applicant.Beneficiaries = null;
            //    allocation.Beneficiary.Applicant.Executors = null;
            //    allocation.Beneficiary.Applicant.Guardians = null;
            //    allocation.Beneficiary.Applicant.Assets = null;
            //    allocation.Beneficiary.Applicant.AssetAllocations = null;
            //    allocation.Beneficiary.Applicant.PaymentTransactions = null;
            //}

            // Simplified cleanup for the single allocation object.
            allocation.Applicant = null; // Clean the top-level applicant
            if (allocation.Asset != null) allocation.Asset.Applicant = null;
            if (allocation.Beneficiary != null) allocation.Beneficiary.Applicant = null;

            return allocation;
        }

        public async Task<BeneficiaryAssetAllocation?> UpdateBeneficiaryAssetAllocationAsync(int applicantId, int id, BeneficiaryAssetAllocation allocation)
        {
            if (id != allocation.Id || applicantId != allocation.ApplicantId)
            {
                return null; // IDs in the route must match the IDs in the body
            }
            var existingAllocation = await _context.BeneficiaryAssetAllocations
                                              .FirstOrDefaultAsync(a => a.Id == id && a.ApplicantId == applicantId);
            if (existingAllocation == null)
            {
                return null; // Allocation not found
            }
            _context.Entry(existingAllocation).CurrentValues.SetValues(allocation);

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
