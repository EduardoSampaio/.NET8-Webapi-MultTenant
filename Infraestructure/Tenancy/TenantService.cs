using Application.Features.Tenancy;
using Finbuckle.MultiTenant;
using Finbuckle.MultiTenant.Abstractions;
using Infraestructure.Contexts;
using Mapster;
using Microsoft.Extensions.DependencyInjection;

namespace Infraestructure.Tenancy
{
    public class TenantService : ITenantService
    {
        private readonly IMultiTenantStore<ABCSchoolTenantInfo> _tenantStore;
        private readonly ApplicationDbSeeder _applicationDbSeeder;
        private readonly IServiceProvider _serviceProvider;

        public TenantService(IMultiTenantStore<ABCSchoolTenantInfo> tenantStore, ApplicationDbSeeder applicationDbSeeder, IServiceProvider serviceProvider)
        {
            _tenantStore = tenantStore;
            _applicationDbSeeder = applicationDbSeeder;
            _serviceProvider = serviceProvider;
        }

        public async Task<string> ActivateAsync(string id)
        {
            var tenantInDb = await _tenantStore.TryGetAsync(id);
            tenantInDb.IsActive = true;
            await _tenantStore.TryUpdateAsync(tenantInDb);
            return tenantInDb.Identifier;
        }

        public async Task<string> CreateTenantAsync(CreateTenantRequest request, CancellationToken cancellationToken)
        {
            var newTenant = new ABCSchoolTenantInfo
            {
                Identifier = request.Identifier,
                Name = request.Name,
                IsActive = true,
                ConnectionString = request.ConnectionString,
                Email = request.Email,
                FirstName = request.FirstName,
                LastName = request.LastName,
                ValiUpTo = request.ValiUpTo,
            };

            await _tenantStore.TryAddAsync(newTenant);
            using var scope = _serviceProvider.CreateScope();
            _serviceProvider.GetRequiredService<IMultiTenantContextSetter>()
                .MultiTenantContext = new MultiTenantContext<ABCSchoolTenantInfo>()
                {
                    TenantInfo = newTenant
                };
            await scope.ServiceProvider.GetRequiredService<ApplicationDbSeeder>().InitializeDatabaseAsync(cancellationToken);

            return newTenant.Identifier;
        }

        public async Task<string> DeactivateAsync(string id)
        {
            var tenantInDb = await _tenantStore.TryGetAsync(id);
            tenantInDb.IsActive = false;
            await _tenantStore.TryUpdateAsync(tenantInDb);
            return tenantInDb.Identifier;
        }

        public async Task<TenantResponse> GetTenantByIdAsync(string id)
        {
            var tenant = await _tenantStore.TryGetAsync(id);
            //return Task.FromResult(new TenantResponse
            //{
            //    Identifier = tenant.Result.Identifier,
            //    Name = tenant.Result.Name,
            //    IsActive = tenant.Result.IsActive,
            //    ConnectionString = tenant.Result.ConnectionString,
            //    Email = tenant.Result.Email,
            //    FirstName = tenant.Result.FirstName,
            //    LastName = tenant.Result.LastName,
            //    ValiUpTo = tenant.Result.ValiUpTo,
            //});

            return tenant.Adapt<TenantResponse>();
        }

        public async Task<List<TenantResponse>> GetTenantsAsync()
        {
            var tenantsInDb = await _tenantStore.GetAllAsync();

            return tenantsInDb.Adapt<List<TenantResponse>>();
        }

        public async Task<string> UpdateSubscriptionAsync(UpdateTenantSubscriptionRequest updateTenantSubscriptionRequest)
        {
            var tenantInDb = await _tenantStore.TryGetAsync(updateTenantSubscriptionRequest.TenantId);
            tenantInDb.ValiUpTo = updateTenantSubscriptionRequest.NewExpiryDate;

            await _tenantStore.TryUpdateAsync(tenantInDb);
            return tenantInDb.Identifier;
        }
    }
}
