using Leadway_RSA_API.Data;
using Leadway_RSA_API.Models;
using Microsoft.EntityFrameworkCore;

namespace Leadway_RSA_API.Services
{
    public class RegistrationService : IRegistrationService
    {
        private readonly ApplicationDbContext _context;

        public RegistrationService(ApplicationDbContext context)
        {
            _context = context;
        }

        // This method was missing from my previous example
        public Task<bool> ValidateSponsorshipKeyAsync(string sponsorshipKey)
        {
            // This is still a mock implementation for now, but it fulfills the interface contract.
            // In a real application, you would check a database for a valid sponsorship key.
            return Task.FromResult(true);
        }

        public async Task<string> GenerateAndSaveKey(int applicantId)
        {
            // Check if a key already exists for this applicant
            var existingKey = await _context.RegistrationKeys.FirstOrDefaultAsync(k => k.ApplicantId == applicantId);

            if (existingKey != null)
            {
                // If it exists, return the existing key instead of creating a new one
                return existingKey.Key;
            }

            // If no key exists, generate and save a new one as before
            var key = Guid.NewGuid().ToString("N");
            var registrationKey = new RegistrationKey
            {
                Key = key,
                ApplicantId = applicantId,
                CreatedAt = DateTime.UtcNow,
                IsUsed = false
            };
            _context.RegistrationKeys.Add(registrationKey);
            await _context.SaveChangesAsync();
            return key;
        }

        public async Task<int?> ValidateKey(string key)
        {
            // The key is now valid for the entire session.
            var registrationKey = await _context.RegistrationKeys.FirstOrDefaultAsync(k => k.Key == key);


            if (registrationKey != null)
            {
                // We can optionally add a check for expiration here
                // if (DateTime.UtcNow > registrationKey.CreatedAt.AddHours(24)) { return null; }

                // We do NOT mark the key as used here.
                return registrationKey.ApplicantId;
            }
            return null;
        }

        public async Task<bool> ResendRegistrationKeyAsync(string emailAddress)
        {
            // Find the applicant and their key.
            var applicant = await _context.Applicants
                .Include(a => a.RegistrationKey)
                .FirstOrDefaultAsync(a => a.EmailAddress == emailAddress);

            // If no active applicant or key is found, we still return true to avoid
            // giving an attacker information about valid email addresses.
            if (applicant == null || applicant.RegistrationKey == null)
            {
                return true;
            }

            var registrationKey = applicant.RegistrationKey.Key;

            // --- You will integrate your email or SMS service here ---
            // Example: send an email with a link like:
            // https://<your-frontend-domain>/continue-registration?key={registrationKey}
            // _emailService.SendEmailAsync(emailAddress, "Continue Your Registration", $"Click here: {link}");

            return true; // Return true as the process was initiated successfully.
        }
    }


    //public class RegistrationService
    //{
    //    private readonly ApplicationDbContext _context;

    //    public RegistrationService(ApplicationDbContext context)
    //    {
    //        _context = context;
    //    }

    //    public async Task<string> GenerateAndSaveKey(int applicantId)
    //    {
    //        var key = Guid.NewGuid().ToString();
    //        var registrationKey = new RegistrationKey()
    //        {
    //            Key = key,
    //            ApplicantId = applicantId,
    //            ExpirationDate = DateTime.UtcNow.AddHours(2) // Key expires after 2 hours
    //        };

    //        _context.RegistrationKeys.Add(registrationKey);
    //        await _context.SaveChangesAsync();
    //        return key;
    //    }

    //    public async Task<int?> ValidateKey(string key)
    //    {
    //        var registrationKey = await _context.RegistrationKeys.FirstOrDefaultAsync(k => k.Key == key && k.ExpirationDate > DateTime.UtcNow);
    //        if (registrationKey == null)
    //        {
    //            return null; // Key is invalid or expired
    //        }
    //        return registrationKey.ApplicantId;
    //    }

    //    /// <summary>
    //    /// Validates a given sponsorship key asynchronously.
    //    /// </summary>
    //    public async Task<bool> ValidateSponsorshipKeyAsync(string sponsorshipKey)
    //    {
    //        // This is where you would connect to a database to verify the key.
    //        // For this example, we'll simulate a valid key check.
    //        await Task.Delay(100);
    //        return !string.IsNullOrEmpty(sponsorshipKey) && sponsorshipKey == "SPONSORSHIP123";
    //    }
    //}
}
