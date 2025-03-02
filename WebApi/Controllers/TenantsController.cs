using Application.Features.Tenancy;
using Application.Features.Tenancy.Commands;
using Application.Features.Tenancy.Queries;
using Infraestructure.Constants;
using Infraestructure.Identity.Auth;
using Microsoft.AspNetCore.Mvc;

namespace WebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TenantsController : BaseApiController
    {
        [HttpPost("add")]
        [ShouldHavePermission(SchoolAction.Create, SchoolFeature.Tenants)]
        public async Task<IActionResult> AddTenantAsync([FromBody] CreateTenantRequest createTenantRequest)
        {
            var response = await Sender.Send(new CreateTenantCommand() { CreateTenant = createTenantRequest });
            if (response.ISuccessful)
            {
                return Ok(response);
            }
            return BadRequest();
        }

        [HttpPut("{tenantId}/activate")]
        [ShouldHavePermission(SchoolAction.Update, SchoolFeature.Tenants)]
        public async Task<IActionResult> ActivateTenantAsync([FromRoute] string tenantId)
        {
            var response = await Sender.Send(new ActivateTenantCommand() { TenantId = tenantId });
            if (response.ISuccessful)
            {
                return Ok(response);
            }
            return BadRequest();
        }

        [HttpPut("{tenantId}/deactivate")]
        [ShouldHavePermission(SchoolAction.Update, SchoolFeature.Tenants)]
        public async Task<IActionResult> DeactivateTenantAsync([FromRoute] string tenantId)
        {
            var response = await Sender.Send(new DeactivateTenantCommand() { TenantId = tenantId });
            if (response.ISuccessful)
            {
                return Ok(response);
            }
            return BadRequest();
        }

        [HttpPut("{tenantId}/subscription")]
        [ShouldHavePermission(SchoolAction.UpgradeSubscription, SchoolFeature.Tenants)]
        public async Task<IActionResult> UpdateTenantSubscriptionAsync([FromRoute] string tenantId, [FromBody] UpdateTenantSubscriptionRequest updateTenantSubscriptionRequest)
        {
            var response = await Sender.Send(new UpdateTenantSubscriptionCommand() { UpdateTenantSubscriptionRequest = updateTenantSubscriptionRequest });
            if (response.ISuccessful)
            {
                return Ok(response);
            }
            return BadRequest();
        }

        [HttpGet("{tenantId}")]
        [ShouldHavePermission(SchoolAction.Read, SchoolFeature.Tenants)]
        public async Task<IActionResult> GetTenantByIdAsync([FromRoute] string tenantId)
        {
            var response = await Sender.Send(new GetTenantByIdQuery() { TenantId = tenantId });
            if (response.ISuccessful)
            {
                return Ok(response);
            }
            return BadRequest();
        }

        [HttpGet]
        [ShouldHavePermission(SchoolAction.Read, SchoolFeature.Tenants)]
        public async Task<IActionResult> GetAllTenantsAsync()
        {
            var response = await Sender.Send(new GetTenantsQuery());
            if (response.ISuccessful)
            {
                return Ok(response);
            }
            return BadRequest();
        }
    }
}
 