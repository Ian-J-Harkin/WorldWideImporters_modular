using Microsoft.EntityFrameworkCore;
using MassTransit;
using WWI_ModularKit.BuildingBlocks.Abstractions;

namespace WWI_ModularKit.BuildingBlocks.Persistence;

public abstract class BaseDbContext<TContext>(
    DbContextOptions<TContext> options, 
    ITenantProvider tenantProvider) 
    : DbContext(options) where TContext : DbContext
{
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // MassTransit Entities
        modelBuilder.AddInboxStateEntity();
        modelBuilder.AddOutboxMessageEntity();
        modelBuilder.AddOutboxStateEntity();

        // Automated Global Query Filter
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            if (typeof(BaseEntity).IsAssignableFrom(entityType.ClrType))
            {
                var method = typeof(BaseDbContext<TContext>)
                    .GetMethod(nameof(SetQueryFilter), System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!
                    .MakeGenericMethod(entityType.ClrType);
                
                method.Invoke(this, [modelBuilder]);
            }
        }
    }

    private void SetQueryFilter<T>(ModelBuilder modelBuilder) where T : BaseEntity
    {
        modelBuilder.Entity<T>().HasQueryFilter(e => e.TenantId == tenantProvider.GetTenantId());
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        foreach (var entry in ChangeTracker.Entries<BaseEntity>())
        {
            if (entry.State == EntityState.Added)
            {
                // Auto-stamp the TenantId before saving. 
                // Throws if missing context.
                entry.Entity.TenantId = tenantProvider.GetTenantId();
            }
        }
        return base.SaveChangesAsync(cancellationToken);
    }
}
