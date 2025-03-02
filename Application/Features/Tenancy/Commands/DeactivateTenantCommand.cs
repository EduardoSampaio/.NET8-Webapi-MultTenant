using Application.Wrappers;
using MediatR;

namespace Application.Features.Tenancy.Commands;

public class DeactivateTenantCommand : IRequest<IResponseWrapper>
{
    public string TenantId { get; set; }
}


public class DeactivateTenantCommandHandler(ITenantService tenantService) : IRequestHandler<ActivateTenantCommand, IResponseWrapper>
{
    private readonly ITenantService _tenantService = tenantService;

    public async Task<IResponseWrapper> Handle(ActivateTenantCommand request, CancellationToken cancellationToken)
    {
        var tenantId = await _tenantService.ActivateAsync(request.TenantId);

        return await ResponseWrapper<string>.SuccessAsync(data: tenantId, message: "Tenant deactivation successfully");
    }
}

