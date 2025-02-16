using Finbuckle.MultiTenant;
using Infraestructure.Contexts;
using Infraestructure.Tenancy;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Infraestructure;
public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<TenantDbContext>(options =>
        {
            options.UseSqlServer(configuration.GetConnectionString("DefaultConnection"));
        }).AddMultiTenant<ABCSchoolTenantInfo>()
         .WithHeaderStrategy(TenancyConstants.TenantIdName)
         .WithClaimStrategy(TenancyConstants.TenantIdName)
         .WithEFCoreStore<TenantDbContext, ABCSchoolTenantInfo>();

        services.AddDbContext<ApplicationDbContext>(options =>
        {
            options.UseSqlServer(configuration.GetConnectionString("DefaultConnection"));
        });

        return services;
    }

    public static IApplicationBuilder UseInfrastructure(this IApplicationBuilder app)
    {
        app.UseMultiTenant();
        return app;
    }
}
