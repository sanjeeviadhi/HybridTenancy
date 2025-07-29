using Microsoft.EntityFrameworkCore;
using System.Data;
using Npgsql;
using HybridTenancy.Persistence.DbContexts;

namespace HybridTenancy.Service.Multitenancy
{
    public class TenantConnectionFactory
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ILogger<TenantConnectionFactory> _logger;

        public TenantConnectionFactory(IHttpContextAccessor httpContextAccessor, ILogger<TenantConnectionFactory> logger)
        {
            _httpContextAccessor = httpContextAccessor;
            _logger = logger;
        }

        public IDbConnection CreateConnection()
        {
            try
            {
                var tenant = (TenantInfo)_httpContextAccessor.HttpContext!.Items["TenantInfo"]!;
                _logger.LogInformation("Creating NpgsqlConnection for tenant: {TenantId}", tenant.TenantId);

                var conn = new NpgsqlConnection(tenant.ConnectionString);
                conn.Open();

                _logger.LogInformation("Connection established successfully for tenant: {TenantId}", tenant.TenantId);
                return conn;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to create or open connection for tenant.");
                throw; // Rethrow to bubble up the exception
            }
        }

        public MultiTenantDbContext CreateDbContext()
        {
            try
            {
                var tenant = (TenantInfo)_httpContextAccessor.HttpContext!.Items["TenantInfo"]!;
                _logger.LogInformation("Creating DbContext for tenant: {TenantId}", tenant.TenantId);

                var optionsBuilder = new DbContextOptionsBuilder<MultiTenantDbContext>();
                optionsBuilder.UseNpgsql(tenant.ConnectionString);

                var context = new MultiTenantDbContext(optionsBuilder.Options, tenant);
                _logger.LogInformation("DbContext created successfully for tenant: {TenantId}", tenant.TenantId);

                return context;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to create DbContext for tenant.");
                throw;
            }
        }
    }
}
