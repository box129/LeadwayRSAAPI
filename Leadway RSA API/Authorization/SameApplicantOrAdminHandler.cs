using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Leadway_RSA_API.Authorization
{
    // The 'int' in the handler indicates that the resource to be authorized is an integer (the applicantId from the URL).
    public class SameApplicantOrAdminHandler : AuthorizationHandler<SameApplicantOrAdminRequirement, int>
    {
        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, SameApplicantOrAdminRequirement requirement, int resource)
        {
            // Check if the user is an Admin. The 'IsInRole' method checks the 'role' claim.
            if (context.User.IsInRole("Admin"))
            {
                context.Succeed(requirement);
                return Task.CompletedTask;
            }

            // Check if the user's ApplicantId claim matches the resource ID from the URL.
            var userApplicantIdClaim = context.User.Claims.FirstOrDefault(c => c.Type == "ApplicantId");
            if (userApplicantIdClaim != null && int.TryParse(userApplicantIdClaim.Value, out int applicantId) && applicantId == resource)
            {
                context.Succeed(requirement);
            }
            else
            {
                // If neither condition is met, the request fails the authorization check.
                context.Fail();
            }

            return Task.CompletedTask;
        }
    }
}
