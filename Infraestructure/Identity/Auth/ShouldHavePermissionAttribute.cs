using Infraestructure.Constants;
using Microsoft.AspNetCore.Authorization;

namespace Infraestructure.Identity.Auth
{
    public class ShouldHavePermissionAttribute : AuthorizeAttribute
    {
        public ShouldHavePermissionAttribute(string action, string feature)
        {
            Policy = SchoolPermission.NameFor(action, feature);
        }
    }
}
