﻿using Application.Wrappers;
using MediatR;

namespace Application.Features.Tenancy.Queries
{
    public class GetTenantsQuery : IRequest<IResponseWrapper>
    {

    }

    public class GetTenantsQueryHandler(ITenantService tenantService) : IRequestHandler<GetTenantsQuery, IResponseWrapper>
    {
        private readonly ITenantService _tenantService = tenantService;

        public async Task<IResponseWrapper> Handle(GetTenantsQuery request, CancellationToken cancellationToken)
        {
            var tenants = await _tenantService.GetTenantsAsync();
            return await ResponseWrapper<IEnumerable<TenantResponse>>.SuccessAsync(data: tenants);
        }
    }
}
