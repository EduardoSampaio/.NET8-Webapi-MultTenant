using Infraestructure.Constants;
using Infraestructure.Identity.Auth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace WebApi.Controllers;

[Route("api/[controller]")]
public class WeatherCast : BaseApiController
{
    [HttpGet("rain")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult> GetWeatherCast()
    {
        return Ok("Rain");
    }

    [HttpGet("storm")]
    [Authorize(Policy = "AdminOnly")]
    public async Task<ActionResult> GetWeatherCast2()
    {
        return Ok("Rain");
    }

    [HttpGet("sunny")]
    [Authorize(Policy = "Permission.Tokens.RefreshToken")]
    public async Task<ActionResult> GetWeatherForecast()
    {
        return Ok("Sunny");
    }
}
