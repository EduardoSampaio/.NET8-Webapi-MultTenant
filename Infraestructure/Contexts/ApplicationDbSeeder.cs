using Finbuckle.MultiTenant.Abstractions;
using Infraestructure.Constants;
using Infraestructure.Identity.Models;
using Infraestructure.Tenancy;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Infraestructure.Contexts;

public class ApplicationDbSeeder
{
    private readonly IMultiTenantContextAccessor<ABCSchoolTenantInfo> _tenantInfoContextAccessor;
    private readonly RoleManager<ApplicationRole> _roleManager;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ApplicationDbContext _applicationDbContext;

    public ApplicationDbSeeder(
        IMultiTenantContextAccessor<ABCSchoolTenantInfo> tenantInfoContextAccessor,
        RoleManager<ApplicationRole> roleManager,
        UserManager<ApplicationUser> userManager,
        ApplicationDbContext applicationDbContext)
    {
        _tenantInfoContextAccessor = tenantInfoContextAccessor;
        _roleManager = roleManager;
        _userManager = userManager;
        _applicationDbContext = applicationDbContext;
    }

    public async Task InitializeDatabaseAsync(CancellationToken cancellationToken)
    {
        if (_applicationDbContext.Database.GetMigrations().Any())
        {
            if ((await _applicationDbContext.Database.GetPendingMigrationsAsync(cancellationToken)).Any())
            {
                await _applicationDbContext.Database.MigrateAsync(cancellationToken);
            }

            if (await _applicationDbContext.Database.CanConnectAsync(cancellationToken))
            {
                await InitializeDefaultRolesAsync(cancellationToken);
                await InitializeAdminUserAsync();
            }
        }
    }

    public async Task InitializeDefaultRolesAsync(CancellationToken cancellationToken)
    {
        foreach (var roleName in RoleContants.DefaultRoles)
        {
            if (await _roleManager.Roles.SingleOrDefaultAsync(role => role.Name == roleName, cancellationToken) is not ApplicationRole incomingRole)
            {
                incomingRole = new ApplicationRole
                {
                    Name = roleName,
                    Description = $"This is the {roleName} role"
                };
                await _roleManager.CreateAsync(incomingRole);
            }

            if (roleName == RoleContants.Basic)
            {
                await AssignPermissionsToRole(SchoolPermissions.Basic, incomingRole, cancellationToken);
            }
            else if (roleName == RoleContants.Admin)
            {
                await AssignPermissionsToRole(SchoolPermissions.Admin, incomingRole, cancellationToken);

                if (_tenantInfoContextAccessor.MultiTenantContext?.TenantInfo?.Id == TenancyConstants.TenantIdName)
                {
                    await AssignPermissionsToRole(SchoolPermissions.Root, incomingRole, cancellationToken);
                }
            }
        }
    }

    private async Task AssignPermissionsToRole(IReadOnlyCollection<SchoolPermission> rolePermissions,
        ApplicationRole applicationRole, CancellationToken cancellationToken)
    {
        var currentClaims = await _roleManager.GetClaimsAsync(applicationRole);

        foreach (var rolePermission in rolePermissions)
        {
            if (!currentClaims.Any(c => c.Type == ClaimContants.Permission && c.Value == rolePermission.Name))
            {
                await _applicationDbContext.RoleClaims.AddAsync(new ApplicationRoleClaim
                {
                    RoleId = applicationRole.Id,
                    ClaimType = ClaimContants.Permission,
                    ClaimValue = rolePermission.Name,
                    Description = rolePermission.Description,
                    Group = rolePermission.Group
                }, cancellationToken);

                await _applicationDbContext.SaveChangesAsync(cancellationToken);
            }
        }
    }

    private async Task InitializeAdminUserAsync()
    {
        var email = _tenantInfoContextAccessor.MultiTenantContext?.TenantInfo?.Email;
        if (string.IsNullOrEmpty(email)) return;

        if (await _userManager.Users.FirstOrDefaultAsync(user => user.Email == email)
            is not ApplicationUser incomingUser)
        {
            incomingUser = new ApplicationUser
            {
                FirstName = TenancyConstants.FirstName,
                LastName = TenancyConstants.LastName,
                Email =  email,
                UserName = email,
                PhoneNumberConfirmed = true,
                EmailConfirmed = true,
                NormalizedEmail = email.ToUpperInvariant(),
                NormalizedUserName = email.ToUpperInvariant(),
                IsActive = true
            };
            var password = new PasswordHasher<ApplicationUser>();
            incomingUser.PasswordHash = password.HashPassword(incomingUser, TenancyConstants.DefaultPasword);
            await _userManager.CreateAsync(incomingUser);
        }

        if (!await _userManager.IsInRoleAsync(incomingUser, RoleContants.Admin))
        {
            await _userManager.AddToRoleAsync(incomingUser, RoleContants.Admin);
        }
    }
}
