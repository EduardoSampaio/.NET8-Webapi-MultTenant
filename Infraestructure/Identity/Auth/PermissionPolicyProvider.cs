﻿using Infraestructure.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;

namespace Infraestructure.Identity.Auth;

public class PermissionPolicyProvider(IOptions<AuthorizationOptions> options) : IAuthorizationPolicyProvider
{
    public DefaultAuthorizationPolicyProvider FallbackPolicyProvider { get; set; } = new DefaultAuthorizationPolicyProvider(options);

    public Task<AuthorizationPolicy> GetDefaultPolicyAsync()
    {
        return FallbackPolicyProvider.GetDefaultPolicyAsync();
    }

    public Task<AuthorizationPolicy> GetFallbackPolicyAsync() 
    {
        return Task.FromResult<AuthorizationPolicy>(null);
    }

    public Task<AuthorizationPolicy> GetPolicyAsync(string permission)
    {
        if (permission.StartsWith(ClaimContants.Permission, StringComparison.OrdinalIgnoreCase)) 
        {
            var policy = new AuthorizationPolicyBuilder();
            policy.AddRequirements(new PermissionRequirement(permission));
            return Task.FromResult(policy.Build());
        }
        else
        {
            return FallbackPolicyProvider.GetPolicyAsync(permission);
        }

    }
}
