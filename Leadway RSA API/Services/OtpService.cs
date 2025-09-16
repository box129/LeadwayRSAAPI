using Leadway_RSA_API.Services;

namespace Leadway_RSA_API.Services
{
    // Dummy Services (For demonstration purposes)
    public class OtpService :IOtpService
    {
        public Task<bool> SendOtpAsync(string email) { return Task.FromResult(true); }
        public Task<bool> VerifyOtpAsync(string email, string otp) { return Task.FromResult(true); }
    }

}





//using Microsoft.Extensions.Caching.Memory;
//using System;
//using System.Threading.Tasks;
//using System.Security.Cryptography;

//namespace Leadway_RSA_API.Services
//{
//    public class OtpService : IOtpService
//    {
//        private readonly IMemoryCache _cache;
//        private readonly ILogger<OtpService> _logger;

//        public OtpService(IMemoryCache cache, ILogger<OtpService> logger)
//        {
//            _cache = cache;
//            _logger = logger;
//        }

//        /// <summary>
//        /// Generates a random 6-digit OTP and stores it in the cache with a 2-minute expiration.
//        /// </summary>
//        public async Task<bool> GenerateAndSendOtpAsync(int applicantId, string email)
//        {
//            try
//            {
//                // Generate a cryptographically secure random 6-digit number
//                var random = new Random();
//                var otp = random.Next(100000, 999999).ToString();

//                // Store the OTP in the cache with a key specific to the applicant,
//                // and a 2-minute expiration time.
//                var cacheEntryOptions = new MemoryCacheEntryOptions()
//                    .SetAbsoluteExpiration(TimeSpan.FromMinutes(2));

//                _cache.Set($"Otp_{applicantId}", otp, cacheEntryOptions);

//                // TODO: Replace with a real email sending service (e.g., SendGrid, Mailgun)
//                _logger.LogInformation($"OTP for Applicant {applicantId} ({email}): {otp}");

//                // Assuming the email was sent successfully.
//                return await Task.FromResult(true);
//            }
//            catch (Exception ex)
//            {
//                _logger.LogError(ex, "Failed to generate and send OTP.");
//                return await Task.FromResult(false);
//            }
//        }

//        /// <summary>
//        /// Verifies a submitted OTP against the one stored in the cache.
//        /// </summary>
//        public async Task<bool> VerifyOtpAsync(int applicantId, string otp)
//        {
//            // Get the stored OTP from the cache
//            if (_cache.TryGetValue($"Otp_{applicantId}", out string? storedOtp))
//            {
//                // Compare the submitted OTP with the stored one.
//                if (storedOtp == otp)
//                {
//                    // Clear the OTP from the cache after a successful verification to prevent reuse.
//                    _cache.Remove($"Otp_{applicantId}");
//                    return await Task.FromResult(true);
//                }
//            }
//            return await Task.FromResult(false);
//        }
//    }
//}
