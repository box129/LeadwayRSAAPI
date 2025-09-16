namespace Leadway_RSA_API.Models
{
    public class RegistrationKey
    {
        public int Id { get; set; }
        public string Key { get; set; }
        public int ApplicantId { get; set; }
        public DateTime CreatedAt { get; set; }
        public bool IsUsed { get; set; }

        public Applicant Applicant { get; set; }
    }
}
