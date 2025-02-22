using Application.Exceptions;
using Finbuckle.MultiTenant.Abstractions;
using Infraestructure.Identity.Models;
using Infraestructure.Tenancy;
using Microsoft.AspNetCore.Identity;


namespace Application.Features.Identity.Tokens
{
    public class TokenService : ITokenService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IMultiTenantContextAccessor<ABCSchoolTenantInfo> _tenantContextAccessor;

        public TokenService(UserManager<ApplicationUser> userManager, IMultiTenantContextAccessor<ABCSchoolTenantInfo> tenantContextAccessor)
        {
            _userManager = userManager;
            _tenantContextAccessor = tenantContextAccessor;
        }

        public async Task<TokenResponse> LoginAsync(TokenRequest request)
        {
            #region Validations
            if (!_tenantContextAccessor.MultiTenantContext.TenantInfo.IsActive) 
            {
                throw new UnauthorizedException(["Tenant subscription is not active."]);
            }

            var userInDb = await _userManager.FindByNameAsync(request.Username) 
                ?? throw new UnauthorizedException(["Authentication not successful."]);

            if(!await _userManager.CheckPasswordAsync(userInDb, request.Password))
            {
                throw new UnauthorizedException(["Incorrect Username or Password"]);
            }

            if(!userInDb.IsActive)
            {
                throw new UnauthorizedException(["User is not active."]);
            }

            if(_tenantContextAccessor.MultiTenantContext.TenantInfo.Id is not TenancyConstants.Root.Id)
            {
                if (_tenantContextAccessor.MultiTenantContext.TenantInfo.ValiUpTo < DateTime.UtcNow)
                {
                    throw new UnauthorizedException(["Tenant Subscription has expired."]);
                }
            }
            #endregion


        }

        public Task<TokenResponse> RefreshTokenAsync(RefreshTokenRequest request)
        {
            throw new NotImplementedException();
        }
    }
}
