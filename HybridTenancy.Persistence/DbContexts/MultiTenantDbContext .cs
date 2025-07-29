using Microsoft.EntityFrameworkCore;
using HybridTenancy.Shared.Enums;
using HybridTenancy.Persistence.Entities;

namespace HybridTenancy.Persistence.DbContexts
{
    public class MultiTenantDbContext : DbContext
    {
        public TenantInfo Tenant { get; }

        public MultiTenantDbContext(DbContextOptions<MultiTenantDbContext> options, TenantInfo tenant)
              : base(options)
        {
            Tenant = tenant;
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            if (Tenant.Mode == TenantMode.Shared)
            {
                modelBuilder.HasDefaultSchema(Tenant.TenantId.ToString().Replace("-", ""));
            }

            base.OnModelCreating(modelBuilder);
        }

        public DbSet<User> Users { get; set; } = null!;

    }
}
