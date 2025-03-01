using Application.Features.Identity.Tokens;
using Application.Features.Identity.Tokens.Queries;
using Infraestructure.Constants;
using Infraestructure.Identity.Auth;
using Infraestructure.OpenApi;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NSwag.Annotations;

namespace WebApi.Controllers
{
    [Route("api/[controller]")]
    public class TokenController : BaseApiController
    {
        [HttpPost("login")]
        [AllowAnonymous]
        [TenantHeader]
        [OpenApiOperation("Used to obtain jwt for login.")]
        public async Task<ActionResult> GetTokenAsync([FromBody] TokenRequest tokenRequest)
        {
            var response = await Sender.Send(new GetTokenQuery() { TokenRequest = tokenRequest });

            if (response.ISuccessful)
            {
                return Ok(response);
            }

            return BadRequest(response);
        }

        [HttpPost("refresh-token")]
        [TenantHeader]
        [OpenApiOperation("Used to generate jwt from refresh token.")]
        [ShouldHavePermission(action: SchoolAction.RefreshToken, feature: SchoolFeature.Tokens)]
        public async Task<ActionResult> GetRefreshTokenAsync([FromBody] RefreshTokenRequest refreshTokenRequest)
        {
            var response = await Sender.Send(new GetRefreshTokenQuery() { RefreshTokenRequest = refreshTokenRequest });

            if (response.ISuccessful)
            {
                return Ok(response);
            }

            return BadRequest(response);
        }
    }
}
