namespace Leadway_RSA_API.Models
{
    public class RefreshToken
    {
        public int Id { get; set; }
        public string Token { get; set; }
        public DateTime ExpirationDate { get; set; }
        public int ApplicantId { get; set; } // Link to the user
        public bool IsRevoked { get; set; }
        // Add a foreign key to the Applicant model
        public virtual Applicant? Applicant { get; set; }
    }
}
