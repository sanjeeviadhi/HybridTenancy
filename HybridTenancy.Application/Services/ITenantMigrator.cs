namespace HybridTenancy.Application.Services
{
    public interface ITenantMigrator
    {
        Task ApplyMigrationsAsync(TenantInfo tenant);
    }
}
