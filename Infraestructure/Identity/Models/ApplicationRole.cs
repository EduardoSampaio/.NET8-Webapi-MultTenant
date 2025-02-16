using Microsoft.AspNetCore.Identity;

namespace Infraestructure.Identity.Models;
public class ApplicationRole: IdentityRole
{
    public string? Description { get; set; }
}

