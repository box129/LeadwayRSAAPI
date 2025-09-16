namespace Leadway_RSA_API.Services
{

    public interface IOtpService
    {
        Task<bool> SendOtpAsync(string email);
        Task<bool> VerifyOtpAsync(string email, string otp);
    }

    //public interface IOtpService
    //{
    //    /// <summary>
    //    /// Generates an OTP, stores it with an expiration, and sends it to the specified email.
    //    /// </summary>
    //    Task<bool> GenerateAndSendOtpAsync(string email);

    //    Task<bool> VerifyOtpAsync(string email, string otp);
    //}
}
