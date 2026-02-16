using Microsoft.EntityFrameworkCore;
using WWI_ModularKit.BuildingBlocks.Persistence;
using WWI_ModularKit.BuildingBlocks.Abstractions;
using WWI_ModularKit.Modules.Sales.Entities;

namespace WWI_ModularKit.Modules.Sales.Persistence;

public class SalesDbContext(DbContextOptions<SalesDbContext> options, ITenantProvider tenantProvider) 
    : BaseDbContext<SalesDbContext>(options, tenantProvider)
{
    public DbSet<Order> Orders { get; set; }
    public DbSet<OrderLine> OrderLines { get; set; }
    public DbSet<Customer> Customers { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("sales");
        base.OnModelCreating(modelBuilder);
    }
}
