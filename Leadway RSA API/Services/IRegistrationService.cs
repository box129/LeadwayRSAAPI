namespace Leadway_RSA_API.Services
{
    public interface IRegistrationService
    {
        Task<bool> ValidateSponsorshipKeyAsync(string sponsorshipKey);

        /// <summary>
        /// Validates a registration key and returns the associated ApplicantId.
        /// </summary>
        /// <param name="key">The registration key to validate.</param>
        /// <returns>The ApplicantId if the key is valid; otherwise, null.</returns>
        Task<int?> ValidateKey(string key);

        /// <summary>
        /// Generates and saves a new, unique registration key for an applicant.
        /// </summary>
        /// <param name="applicantId">The ID of the applicant.</param>
        /// <returns>The newly generated registration key string.</returns>
        Task<string> GenerateAndSaveKey(int applicantId);

        Task<bool> ResendRegistrationKeyAsync(string emailAddress);
    }
}
