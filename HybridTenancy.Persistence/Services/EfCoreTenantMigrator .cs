using HybridTenancy.Application.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using HybridTenancy.Persistence.DbContexts;

namespace Persistence.Services
{
    public class EfCoreTenantMigrator : ITenantMigrator
    {
        private readonly ILogger<EfCoreTenantMigrator> _logger;

        public EfCoreTenantMigrator(ILogger<EfCoreTenantMigrator> logger)
        {
            _logger = logger;
        }

        public async Task ApplyMigrationsAsync(TenantInfo tenant)
        {
            try
            {
                var optionsBuilder = new DbContextOptionsBuilder<MultiTenantDbContext>();
                optionsBuilder.UseNpgsql(tenant.ConnectionString);

                using var context = new MultiTenantDbContext(optionsBuilder.Options, tenant);

                _logger.LogInformation("Applying EF Core migrations for tenant {TenantId}", tenant.TenantId);
                await context.Database.MigrateAsync();
                _logger.LogInformation("Migrations applied successfully for tenant {TenantId}", tenant.TenantId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Migration failed for tenant {TenantId}", tenant.TenantId);
                throw;
            }
        }
    }
}
