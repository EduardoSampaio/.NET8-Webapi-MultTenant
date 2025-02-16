using Microsoft.AspNetCore.Identity;

namespace Infraestructure.Identity.Models;

public class ApplicationRoleClaim : IdentityRoleClaim<string>
{
    public string? Description { get; set; }
    public string? Group { get; set; }
}

