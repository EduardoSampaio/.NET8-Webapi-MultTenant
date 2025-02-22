using Finbuckle.MultiTenant;
using Infraestructure.Contexts;
using Infraestructure.Identity.Auth;
using Infraestructure.Identity.Models;
using Infraestructure.Tenancy;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
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

        services.AddTransient<ITenantDbSeeder, TenantDbSeeder>();
        services.AddTransient<ApplicationDbSeeder>();
        services.AddIdentityService(configuration);
        services.AddPermissions();

        return services;
    }

    public static async Task AddDatabaseInitializeAsync(this IServiceProvider serviceProvider, CancellationToken cancellationToken = default)
    {
        using var scope = serviceProvider.CreateScope();
        await scope.ServiceProvider
                   .GetRequiredService<ITenantDbSeeder>()
                   .InitializeDatabaseAsync(cancellationToken);
    }

    internal static IServiceCollection AddIdentityService(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddIdentity<ApplicationUser, ApplicationRole>(options =>
        {
            options.Password.RequireNonAlphanumeric = false;
            options.Password.RequireDigit = false;
            options.Password.RequireLowercase = false;
            options.Password.RequireUppercase = false;
            options.Password.RequiredLength = 8;
            options.User.RequireUniqueEmail = true;
        })
        .AddEntityFrameworkStores<ApplicationDbContext>()
        .AddDefaultTokenProviders();

        return services;
    }

    public static IApplicationBuilder UseInfrastructure(this IApplicationBuilder app)
    {
        app.UseMultiTenant();
        return app;
    }

    internal static IServiceCollection AddPermissions(this IServiceCollection services)
    {
        services.AddSingleton<IAuthorizationPolicyProvider, PermissionPolicyProvider>();
        services.AddSingleton<IAuthorizationHandler, PermissionAuthorizationHandler>();
        return services;
    }
}
