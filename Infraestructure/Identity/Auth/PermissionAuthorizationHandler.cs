using Infraestructure.Constants;
using Microsoft.AspNetCore.Authorization;

namespace Infraestructure.Identity.Auth
{
    public class PermissionAuthorizationHandler : AuthorizationHandler<PermissionRequirement>
    {
        protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, PermissionRequirement requirement)
        {
            var permissions = context.User
                                    .Claims
                                    .Where(claim => claim.Type == ClaimContants.Permission && claim.Value == requirement.Permission);
            if (permissions.Any())
            { 
                context.Succeed(requirement);
                await Task.CompletedTask;
            }
        }
    }
}
