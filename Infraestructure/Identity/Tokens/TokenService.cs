using Application.Exceptions;
using Finbuckle.MultiTenant.Abstractions;
using Infraestructure.Constants;
using Infraestructure.Identity.Models;
using Infraestructure.Tenancy;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;


namespace Application.Features.Identity.Tokens
{
    public class TokenService : ITokenService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<ApplicationRole> _roleManager;
        private readonly IMultiTenantContextAccessor<ABCSchoolTenantInfo> _tenantContextAccessor;

        public TokenService(UserManager<ApplicationUser> userManager, RoleManager<ApplicationRole> roleManager, 
            IMultiTenantContextAccessor<ABCSchoolTenantInfo> tenantContextAccessor)
        {
            _userManager = userManager;
            _roleManager = roleManager;
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

            if (!await _userManager.CheckPasswordAsync(userInDb, request.Password))
            {
                throw new UnauthorizedException(["Incorrect Username or Password"]);
            }

            if (!userInDb.IsActive)
            {
                throw new UnauthorizedException(["User is not active."]);
            }

            if (_tenantContextAccessor.MultiTenantContext.TenantInfo.Id is not TenancyConstants.Root.Id)
            {
                if (_tenantContextAccessor.MultiTenantContext.TenantInfo.ValiUpTo < DateTime.UtcNow)
                {
                    throw new UnauthorizedException(["Tenant Subscription has expired."]);
                }
            }
            #endregion

            return await GenerateTokenAndUpdateUserAsync(userInDb);

        }

        public Task<TokenResponse> RefreshTokenAsync(RefreshTokenRequest request)
        {
            throw new NotImplementedException();
        }

        public async Task<TokenResponse> GenerateTokenAndUpdateUserAsync(ApplicationUser user)
        {
            var newjwt = await GenerateToken(user);

            user.RefreshToken = GenerateRefreshToken();
            user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(1);

            await _userManager.UpdateAsync(user);

            return new TokenResponse
            {
                Jwt = newjwt,
                RefreshToken = user.RefreshToken,
                RefreshTokenExpireDate = user.RefreshTokenExpiryTime
            };
        }

        private async Task<string> GenerateToken(ApplicationUser user)
        {
            var signingCredentials = GetSigningCredentials();
            var claims = await GetUserClaims(user);
            return GenerateEncryptedToken(signingCredentials, claims);
        }

        private string GenerateEncryptedToken(SigningCredentials signingCredentials, IEnumerable<Claim> claims)
        {
            var token = new JwtSecurityToken(
                 claims: claims,
                 expires: DateTime.UtcNow.AddMinutes(60),
                 signingCredentials: signingCredentials
             );
            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        private SigningCredentials GetSigningCredentials()
        {
            var key = Encoding.UTF8.GetBytes("MySuperSecret");
            var secret = new SymmetricSecurityKey(key);
            return new SigningCredentials(secret, SecurityAlgorithms.HmacSha256);
        }

        private async Task<IEnumerable<Claim>> GetUserClaims(ApplicationUser user)
        {
            var userClaims = await _userManager.GetClaimsAsync(user);
            var userRoles = await _userManager.GetRolesAsync(user);

            var roleClaims = new List<Claim>();
            var permissionClaims = new List<Claim>();

            foreach (var role in userRoles)
            {
                roleClaims.Add(new Claim(ClaimTypes.Role, role));
                var currentRole = await _roleManager.FindByNameAsync(role);

                var allPermissionForCurrentROle = await _roleManager.GetClaimsAsync(currentRole);
                permissionClaims.AddRange(allPermissionForCurrentROle);

            }

            var claims = new List<Claim>
            {
                new(ClaimTypes.NameIdentifier, user.Id),
                new(ClaimTypes.Name, user.FirstName),
                new(ClaimTypes.Email, user.Email),
                new(ClaimTypes.Surname, user.LastName),
                new(ClaimTypes.MobilePhone, user.PhoneNumber ?? string.Empty),
                new(ClaimContants.Tenant, _tenantContextAccessor.MultiTenantContext.TenantInfo.Id),

            }.Union(userClaims)
             .Union(roleClaims)
             .Union(permissionClaims);

            return claims;

        }

        private string GenerateRefreshToken()
        {
            var randomNumber = new byte[32];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(randomNumber);
            return Convert.ToBase64String(randomNumber);
        }
    }
}
