namespace Application.Features.Tenancy
{
    public interface ITenantService
    {
        Task<string> CreateTenantAsync(CreateTenantRequest request, CancellationToken cancellationToken);
        Task<string> ActivateAsync(string id);
        Task<string> DeactivateAsync(string id);
        Task<string> UpdateSubscriptionAsync(UpdateTenantSubscriptionRequest updateTenantSubscriptionRequest);
        Task<List<TenantResponse>> GetTenantsAsync();
        Task<TenantResponse> GetTenantByIdAsync(string id);
    }
}
