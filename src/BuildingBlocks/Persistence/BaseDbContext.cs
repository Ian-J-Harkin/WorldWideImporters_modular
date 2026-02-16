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
                modelBuilder.Entity(entityType.ClrType)
                    .HasQueryFilter(GenerateQueryFilter(entityType.ClrType));
            }
        }
    }

    private System.Linq.Expressions.LambdaExpression GenerateQueryFilter(Type type)
    {
        // The query filter is evaluated per-request. ITenantProvider.GetTenantId()
        // will now throw if the tenant is missing, fulfilling the "Strict Enforcement" guardrail.
        
        var parameter = System.Linq.Expressions.Expression.Parameter(type, "e");
        var property = System.Linq.Expressions.Expression.Property(parameter, nameof(BaseEntity.TenantId));
        
        // Reference the provider to evaluation per-request
        var provider = System.Linq.Expressions.Expression.Constant(tenantProvider);
        var getTenantId = System.Linq.Expressions.Expression.Call(provider, typeof(ITenantProvider).GetMethod(nameof(ITenantProvider.GetTenantId))!);
        
        var comparison = System.Linq.Expressions.Expression.Equal(property, getTenantId);

        return System.Linq.Expressions.Expression.Lambda(comparison, parameter);
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
