using Microsoft.EntityFrameworkCore;
using WWI_ModularKit.BuildingBlocks.Persistence;
using WWI_ModularKit.BuildingBlocks.Abstractions;
using WWI_ModularKit.Modules.Warehouse.Entities;

namespace WWI_ModularKit.Modules.Warehouse.Persistence;

public class WarehouseDbContext(DbContextOptions<WarehouseDbContext> options, ITenantProvider tenantProvider) 
    : BaseDbContext<WarehouseDbContext>(options, tenantProvider)
{
    public DbSet<StockItem> StockItems { get; set; }
    public DbSet<StockHolding> StockHoldings { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.HasDefaultSchema("warehouse");

        // Seed Data
        modelBuilder.Entity<StockItem>().HasData(
            new StockItem { Id = 220, Name = "USB Missile Launcher (Green)" }
        );

        modelBuilder.Entity<StockHolding>().HasData(
            new StockHolding 
            { 
                Id = Guid.NewGuid(),
                TenantId = Guid.Parse("8db1620a-8640-410a-8651-f0945934188b"), // WWI Sample Tenant
                StockItemId = 220, 
                QuantityOnHand = 10 
            }
        );
    }
}
