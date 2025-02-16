using Domain.Entities;
using Finbuckle.MultiTenant.Abstractions;
using Infraestructure.Tenancy;
using Microsoft.EntityFrameworkCore;

namespace Infraestructure.Contexts;
public class ApplicationDbContext : BaseDbContext
{
    public ApplicationDbContext(IMultiTenantContextAccessor<ABCSchoolTenantInfo> multiTenantContextAccessor, DbContextOptions<ApplicationDbContext> options) : 
        base(multiTenantContextAccessor, options)
    {
    }

    public DbSet<School> Schools => Set<School>();
}
