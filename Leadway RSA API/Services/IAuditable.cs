namespace Leadway_RSA_API.Services
{
    public interface IAuditable
    {
        DateTime CreatedDate { get; set; }
        DateTime LastModifiedDate { get; set; }
    }
}
