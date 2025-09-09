using Leadway_RSA_API.Services;

namespace Leadway_RSA_API.Models
{
    public class PersonalDetails : IAuditable
    {
        public int Id { get; set; }
        public int ApplicantId { get; set; }
        public string PlaceOfBirth { get; set; }
        public string Religion { get; set; }
        public string Gender { get; set; }
        public string HomeAddress { get; set; }
        public string State { get; set; }
        public string City { get; set; }

        // Paths to files saved locally
        public string PassportPhotoPath { get; set; }
        public string SignaturePath { get; set; }

        public DateTime CreatedDate { get; set; }
        public DateTime LastModifiedDate { get; set; }

        // --- Navigation Property (for Entity Framework Core relationship) ---
        // This property represents the "one" side of the one-to-many relationship with Applicant.
        public virtual Applicant? Applicant { get; set; }
    }
}
