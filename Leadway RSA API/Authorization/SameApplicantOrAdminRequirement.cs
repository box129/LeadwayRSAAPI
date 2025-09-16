using Microsoft.AspNetCore.Authorization;

namespace Leadway_RSA_API.Authorization
{
    public class SameApplicantOrAdminRequirement : IAuthorizationRequirement
    {
        // This class is intentionally empty as it only serves as a marker for the policy.
    }
}
