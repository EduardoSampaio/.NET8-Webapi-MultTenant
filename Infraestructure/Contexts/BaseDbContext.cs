using Finbuckle.MultiTenant.Abstractions;
using Finbuckle.MultiTenant.EntityFrameworkCore;
using Infraestructure.Identity.Models;
using Infraestructure.Tenancy;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Reflection;

namespace Infraestructure.Contexts;
public class BaseDbContext: MultiTenantIdentityDbContext<
    ApplicationUser, 
    ApplicationRole, 
    string, 
    IdentityUserClaim<string>, 
    IdentityUserRole<string>,
    IdentityUserLogin<string>,
    ApplicationRoleClaim,
    IdentityUserToken<string>>
{
    private new ABCSchoolTenantInfo? TenantInfo {get; set; }

    protected BaseDbContext(IMultiTenantContextAccessor<ABCSchoolTenantInfo> multiTenantContextAccessor,
        DbContextOptions options) : base(multiTenantContextAccessor, options)
    {
    }
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        base.OnConfiguring(optionsBuilder);

        if (!string.IsNullOrEmpty(TenantInfo?.ConnectionString))
        {
            optionsBuilder.UseSqlServer(TenantInfo.ConnectionString, options =>
            {
                options.MigrationsAssembly(Assembly.GetExecutingAssembly().FullName);
            });
        }
    }
}
