using Dapper;
using HybridTenancy.Shared.Enums;
using HybridTenancy.Shared.Utils;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Npgsql;
using System.Dynamic;

namespace HybridTenancy.Application.Services
{
    public class TenantService : ITenantService
    {
        private readonly IConfiguration _config;
        private readonly string _adminConnectionString;
        private readonly ITenantMigrator _tenantMigrator;
        private readonly ILogger<TenantService> _logger;

        public TenantService(IConfiguration config, ITenantMigrator tenantMigrator, ILogger<TenantService> logger)
        {
            _config = config;
            _tenantMigrator = tenantMigrator;
            _logger = logger;
            _adminConnectionString = _config.GetConnectionString("AdminConnection")!;
        }
        public async Task<TenantInfo?> GetTenantByIdentifierAsync(string identifier)
        {
            try
            {
                using var conn = new NpgsqlConnection(_adminConnectionString);
                var sql = "SELECT * FROM public.\"Tenants\" WHERE \"Identifier\" = @Identifier";
                return await conn.QuerySingleOrDefaultAsync<TenantInfo>(sql, new { Identifier = identifier });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get tenant by identifier {Identifier}", identifier);
                throw;
            }
        }

        public async Task<TenantInfo?> GetTenantAsync(Guid tenantId)
        {
            try
            {
                using var conn = new NpgsqlConnection(_adminConnectionString);
                var sql = "SELECT * FROM public.\"Tenants\" WHERE \"TenantId\" = @TenantId";
                var tenant = await conn.QuerySingleOrDefaultAsync<TenantInfo>(sql, new { TenantId = tenantId });

                if (tenant == null)
                    _logger.LogWarning("Tenant {TenantId} not found", tenantId);
                else
                    _logger.LogInformation("Tenant {TenantId} retrieved", tenantId);

                return tenant;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching tenant {TenantId}", tenantId);
                throw;
            }
        }

        public async Task<List<TenantInfo>> GetAllTenantsAsync()
        {
            try
            {
                using var conn = new NpgsqlConnection(_adminConnectionString);
                var sql = "SELECT * FROM public.\"Tenants\"";
                var result = await conn.QueryAsync<TenantInfo>(sql);

                _logger.LogInformation("Retrieved list of all tenants");
                return result.ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving all tenants");
                throw;
            }
        }

        public async Task RegisterTenantAsync(TenantInfo tenant)
        {
            try
            {
                if (tenant.TenantId == null || tenant.TenantId == Guid.Empty)
                    tenant.TenantId = Guid.NewGuid();

                var tenantIdStr = tenant.TenantId.ToString().Replace("-", "");
                var tenantDbName = $"tenant_{tenantIdStr}";
                tenant.CreatedOn = DateTime.UtcNow;

                if (!string.IsNullOrWhiteSpace(tenant.Duration))
                {
                    tenant.ValidTill = DurationParser.ParseToDate(tenant.Duration);
                }

                using var adminConn = new NpgsqlConnection(_adminConnectionString);
                await adminConn.OpenAsync();

                if (tenant.Mode == TenantMode.Isolated)
                {
                    var createDbSql = $"CREATE DATABASE \"{tenantDbName}\"";
                    await adminConn.ExecuteAsync(createDbSql);

                    tenant.ConnectionString = $"Host=localhost;Database={tenantDbName};Username=postgres;Password=12345";
                    _logger.LogInformation("Created isolated DB for tenant {TenantId}", tenant.TenantId);
                }
                else if (tenant.Mode == TenantMode.Shared)
                {
                    tenant.ConnectionString = _adminConnectionString;
                    var schemaName = tenantIdStr;
                    var createSchemaSql = $"CREATE SCHEMA IF NOT EXISTS \"{schemaName}\"";
                    await adminConn.ExecuteAsync(createSchemaSql);
                    _logger.LogInformation("Created schema {Schema} for shared tenant {TenantId}", schemaName, tenant.TenantId);
                }
                else
                {
                    throw new InvalidOperationException("Invalid tenant mode specified.");
                }

                var insertSql = @"
                INSERT INTO public.""Tenants"" 
                (""TenantId"", ""Identifier"", ""ConnectionString"", ""Mode"", ""ValidTill"", ""CreatedOn"") 
                VALUES (@TenantId, @Identifier, @ConnectionString, @Mode, @ValidTill, @CreatedOn);";


                await adminConn.ExecuteAsync(insertSql, tenant);

                _logger.LogInformation("Tenant {TenantId} metadata saved", tenant.TenantId);

                await _tenantMigrator.ApplyMigrationsAsync(tenant);

                _logger.LogInformation("Migrations applied for tenant {TenantId}", tenant.TenantId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error registering tenant");
                throw;
            }
        }

    }
}
