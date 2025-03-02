using Application;
using Application.Features.Identity.Tokens;
using Application.Wrappers;
using Finbuckle.MultiTenant;
using Infraestructure.Constants;
using Infraestructure.Contexts;
using Infraestructure.Identity.Auth;
using Infraestructure.Identity.Models;
using Infraestructure.OpenApi;
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
using NSwag;
using NSwag.Generation.Processors.Security;
using System.Net;
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
        services.AddOpenApiDocumentation(configuration);

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
        app.UseAuthentication();
        app.UseMultiTenant();
        app.UseAuthorization();
        app.UseOpenApiDocumentation();
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
        services.AddAuthorization(options =>
        {
            SchoolPermissions.All.ToList().ForEach(permission =>
            {
                Console.WriteLine($"Permission {permission.Name} Policy {ClaimContants.Permission} {permission.Name}");
                options.AddPolicy(permission.Name, policy => policy.RequireClaim(ClaimContants.Permission, permission.Name));
            });

            options.AddPolicy("AdminOnly", policy => policy.RequireRole("Admin"));
        });

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
                        if (!context.Response.HasStarted)
                        {
                            context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                            context.Response.ContentType = "application/json";

                            var result = JsonSerializer.Serialize(ResponseWrapper.Fail("Token has expired."));
                            return context.Response.WriteAsync(result);
                        }               
                    }
                    else
                    {
                        if (!context.Response.HasStarted)
                        {
                            context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                            context.Response.ContentType = "application/json";

                            var result = JsonSerializer.Serialize(ResponseWrapper.Fail("An unhandled error has occured."));
                            return context.Response.WriteAsync(result);
                        }
                    }
                    return Task.CompletedTask;
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
                    if (!context.Response.HasStarted)
                    {
                        context.Response.StatusCode = (int)HttpStatusCode.Forbidden;
                        context.Response.ContentType = "application/json";
                        var result = JsonSerializer.Serialize(ResponseWrapper.Fail("You are not authorized to access this resource."));
                        return context.Response.WriteAsync(result);
                    }
                    return Task.CompletedTask;
                }
            };
        });

        return services;
    }

    internal static IServiceCollection AddOpenApiDocumentation(this IServiceCollection services, IConfiguration config)
    {
        var swaggerSettings = config.GetSection(nameof(SwaggerSettings)).Get<SwaggerSettings>();
        services.AddEndpointsApiExplorer();
        _ = services.AddOpenApiDocument((document, ServiceProvider) =>
        {
            document.PostProcess = doc =>
            {
                doc.Info.Title = swaggerSettings.Title;
                doc.Info.Description = swaggerSettings.Description;
                doc.Info.Version = swaggerSettings.Version;
                doc.Info.Contact = new OpenApiContact
                {
                    Name = swaggerSettings.ContactName,
                    Email = swaggerSettings.ContactEmail,
                    Url = swaggerSettings.ContactUrl
                };
                doc.Info.License = new OpenApiLicense
                {
                    Name = swaggerSettings.LicenseName,
                    Url = swaggerSettings.LicenseUrl
                };
            };

            document.AddSecurity("Bearer", Enumerable.Empty<string>(), new OpenApiSecurityScheme
            {
                Type = OpenApiSecuritySchemeType.ApiKey,
                Name = "Authorization",
                In = OpenApiSecurityApiKeyLocation.Header,
                Description = "JWT Authorization header using the Bearer scheme.",
                Scheme = JwtBearerDefaults.AuthenticationScheme,
                BearerFormat = "JWT",

            });

            document.OperationProcessors.Add(new AspNetCoreOperationSecurityScopeProcessor());
            document.OperationProcessors.Add(new SwaggerGlobalAuthProcessor());
            document.OperationProcessors.Add(new SwaggerHeaderAttributeProcessor());
        });


        return services;
    }

    internal static IApplicationBuilder UseOpenApiDocumentation(this IApplicationBuilder app)
    {
        app.UseOpenApi();
        app.UseSwaggerUi(options =>
        {
            options.DefaultModelExpandDepth = -1;
            options.DocExpansion = "none";
            options.TagsSorter = "alpha";
        });
        return app;
    }
}
