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
    public class AssetService : IAssetService
    {
        private readonly ApplicationDbContext _context;
        // In a real application, you might inject a logger as well
        // private readonly ILogger<ApplicantService> _logger;

        public AssetService(ApplicationDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Adds a new Asset record for a specific Applicant.
        /// </summary>
        public async Task<Asset?> AddAssetAsync(int applicantId, CreateAssetDto assetDto)
        {
            // 2. Check if the Applicant exists
            var applicant = await _context.Applicants.FindAsync(applicantId);
            if (applicant == null)
            {
                return null;
                //return NotFound($"Applicant with ID {applicantId} not found."); // Returns 404
            }

            var asset = new Asset()
            {
                ApplicantId = applicantId,
                Name = assetDto.Name,
                RSAPin = assetDto.RSAPin,
                PFA = assetDto.PFA,
                SalaryBankName = assetDto.SalaryBankName,
                SalaryAccountNumber = assetDto.SalaryAccountNumber,
            };
            // It's good practice to clear the navigation property to avoid
            // any unintended side effects in Entity Framework Core.
            asset.AssetAllocations = new List<BeneficiaryAssetAllocation>();

            _context.Assets.Add(asset);
            await _context.SaveChangesAsync();

            // Optional: Update the Applicant's LastModifiedDate as well
            applicant.LastModifiedDate = DateTime.UtcNow;
            await _context.SaveChangesAsync(); // Save the update to the applicant

            return asset;
        }

        public async Task<List<Asset>> GetAssetsByApplicantIdAsync(int applicantId)
        {
            var assets = await _context.Assets
                .Where(a => a.ApplicantId == applicantId)
                .ToListAsync();

            return assets; // Returns 200 OK with the list of assets
        }

        public async Task<Asset?> GetAssetAsync(int id)
        {
            // Find the asset by its primary Key
            return await _context.Assets.FindAsync(id);
        }

        public async Task<Asset?> UpdateAssetAsync(int applicantId, int id, UpdateAssetDto assetDto)
        {
           // Find the existing Assset in the database, if not found return 404
            var existingAsset = await _context.Assets.FirstOrDefaultAsync(a => a.Id == id && a.ApplicantId == applicantId);

            if (existingAsset == null)
            {
                return null;
                //return NotFound($"Asset with ID{id} not found or does not belong to Applicant ID {applicantId}.");
            }

            // Apply the updates from the DTO to the existing model.
            // Only update properties that have a value in the DTO.
            if (assetDto.Name != null)
            {
                existingAsset.Name = assetDto.Name;
            }
            if (assetDto.RSAPin != null)
            {
                existingAsset.RSAPin = assetDto.RSAPin;
            }
            if (assetDto.PFA != null)
            {
                existingAsset.PFA = assetDto.PFA;
            }
            if (assetDto.SalaryBankName != null)
            {
                existingAsset.SalaryBankName = assetDto.SalaryBankName;
            }
            if (assetDto.SalaryAccountNumber != null)
            {
                existingAsset.SalaryAccountNumber = assetDto.SalaryAccountNumber;
            }

            try
            {
                // Save changes to the database
                await _context.SaveChangesAsync();

                var applicant = await _context.Applicants.FindAsync(applicantId);
                if (applicant != null)
                {
                    applicant.LastModifiedDate = DateTime.UtcNow;
                    await _context.SaveChangesAsync();
                }
                return existingAsset;
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!AssetExists(id, applicantId)) // Helper to check if the asset actually exist
                {
                    return null;
                    //return NotFound($"Assets with ID {id} not found or does not belong to the Applicant ID {applicantId}.");
                }
                else
                {
                    throw;
                }
            }
        }

        public async Task<bool> DeleteAssetAsync(int applicantId, int id)
        {
            // 1. Check if the asset exists in the database, if not return 404
            var asset = await _context.Assets.FirstOrDefaultAsync(a => a.Id == id && a.ApplicantId == applicantId);
            if (asset == null)
            {
                return false;
                //return NotFound($"Asset with ID {id} not found or does not belong to Applicant ID {applicantId}.");
            }

            // 2. Remove the Asset. EF Core handles the removal from the database
            _context.Assets.Remove(asset);
            await _context.SaveChangesAsync();

            // 3. Find the Applicant using the Aplicant ID and modify the lastModifiedDate
            var applicant = await _context.Applicants.FindAsync(applicantId);
            if (applicant != null)
            {
                applicant.LastModifiedDate = DateTime.UtcNow;
                await _context.SaveChangesAsync();
            }
            return true; 
        }



        // Helper method (add this inside your ApplicantsController class)
        private bool AssetExists(int id, int applicantId)
        {
            return _context.Assets.Any(e => e.Id == id && e.ApplicantId == applicantId);
        }
    }
}
