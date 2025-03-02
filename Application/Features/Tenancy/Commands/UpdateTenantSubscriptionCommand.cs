using Application.Wrappers;
using MediatR;

namespace Application.Features.Tenancy.Commands
{
    public class UpdateTenantSubscriptionCommand : IRequest<IResponseWrapper>
    {
        public UpdateTenantSubscriptionRequest UpdateTenantSubscriptionRequest { get; set; }
    }

    public class UpdateTenantSubscriptionCommandHandler(ITenantService tenantService) : IRequestHandler<UpdateTenantSubscriptionCommand, IResponseWrapper>
    {
        private readonly ITenantService _tenantService = tenantService;

        public async Task<IResponseWrapper> Handle(UpdateTenantSubscriptionCommand request, CancellationToken cancellationToken)
        {
           var tenantId = await _tenantService.UpdateSubscriptionAsync(request.UpdateTenantSubscriptionRequest);

            return await ResponseWrapper<string>.SuccessAsync(data: tenantId, "Tenant subscription successful.");
        }
    }
}
