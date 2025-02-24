using Application;
using Application.Features.Identity.Tokens;
using Application.Wrappers;
using Finbuckle.MultiTenant;
using Infraestructure.Constants;
using Infraestructure.Contexts;
using Infraestructure.Identity.Auth;
using Infraestructure.Identity.Models;
using Infraestructure.Tenancy;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using System.Net;
using System.Reflection;
using System.Text;
using System.Text.Json;

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

        services.AddScoped<ITokenService, TokenService>();

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

    public static JwtSettings GetJwtSettings(this IServiceCollection services, IConfiguration configuration)
    {
        var jwtSettingsConfig =  configuration.GetSection(nameof(JwtSettings));
        services.Configure<JwtSettings>(jwtSettingsConfig);

        return jwtSettingsConfig.Get<JwtSettings>();
    }

    public static IServiceCollection AddJwtAuthentication(this IServiceCollection services, JwtSettings jwtSettings)
    {
        var key = Encoding.ASCII.GetBytes(jwtSettings.Secret);
        services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer(options =>
        {
            options.RequireHttpsMetadata = false;
            options.SaveToken = true;
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                ValidateIssuer = false,
                ValidateAudience = false,
                ClockSkew = TimeSpan.Zero,
                ValidateLifetime = true,
                IssuerSigningKey = new SymmetricSecurityKey(key),
            };

            options.Events = new JwtBearerEvents
            {
                OnAuthenticationFailed = context =>
                {
                    if (context.Exception is SecurityTokenExpiredException)
                    {
                        context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                        context.Response.ContentType = "application/json";

                        var result = JsonSerializer.Serialize(ResponseWrapper.Fail("Token has expired."));
                        return context.Response.WriteAsync(result);
                    }
                    else
                    {
                        context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                        context.Response.ContentType = "application/json";

                        var result = JsonSerializer.Serialize(ResponseWrapper.Fail("An unhandled error has occured."));
                        return context.Response.WriteAsync(result);
                    }
    
                },
                OnChallenge = context =>
                {
                    context.HandleResponse();
                    if (!context.Response.HasStarted)
                    {
                        context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                        context.Response.ContentType = "application/json";
                        var result = JsonSerializer.Serialize(ResponseWrapper.Fail("You are not Authorized."));
                        return context.Response.WriteAsync(result);
                    }
                    return Task.CompletedTask;
                },
                OnForbidden = context =>
                {
                    context.Response.StatusCode = (int)HttpStatusCode.Forbidden;
                    context.Response.ContentType = "application/json";
                    var result = JsonSerializer.Serialize(ResponseWrapper.Fail("You are not authorized to access this resource."));
                    return context.Response.WriteAsync(result);
                }
            };
        });

        services.AddAuthorization(options =>
        {
            foreach (var prop in typeof(SchoolPermissions).GetNestedTypes().SelectMany(type => 
            type.GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy)))
            {
                var propertyValue = prop.GetValue(null);
                if(propertyValue is not null)
                {
                    options.AddPolicy(propertyValue.ToString(), policy => policy.RequireClaim(ClaimContants.Permission, propertyValue.ToString()));
                }
            }
        });
        return services;
    }
}
