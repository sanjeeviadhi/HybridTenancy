
namespace HybridTenancy.Application.Services
{
    public interface ITenantService
    {
        Task<TenantInfo?> GetTenantAsync(Guid tenantId);
        Task RegisterTenantAsync(TenantInfo tenant);
        Task<List<TenantInfo>> GetAllTenantsAsync();
        Task<TenantInfo?> GetTenantByIdentifierAsync(string identifier);
    }
}