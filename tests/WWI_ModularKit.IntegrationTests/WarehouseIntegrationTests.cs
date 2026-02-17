using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using WWI_ModularKit.Modules.Sales.Features.Orders.CreateOrder;
using WWI_ModularKit.Modules.Sales.Persistence;
using WWI_ModularKit.Modules.Warehouse.Persistence;
using Xunit;

namespace WWI_ModularKit.IntegrationTests;

[Collection("IntegrationTests")]
public class WarehouseIntegrationTests : BaseIntegrationTest
{
    public WarehouseIntegrationTests(IntegrationTestFactory factory) : base(factory)
    {
    }

    [Fact]
    public async Task CreateOrder_ShouldReserveStock_AndConfirmOrder()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        SetTenant(tenantId); // Set global tenant for this test

        // 1. Seed Stock in Warehouse UseInMemoryDatabase
        using (var scope = Services.CreateScope())
        {
            var warehouseContext = scope.ServiceProvider.GetRequiredService<WarehouseDbContext>();
            
            // Add unique item for this test
            var stockItem = new Modules.Warehouse.Entities.StockItem { Id = 999, Name = "Test Item" };
            var stockHolding = new Modules.Warehouse.Entities.StockHolding 
            { 
                Id = Guid.NewGuid(),
                StockItemId = 999, 
                QuantityOnHand = 10, 
                TenantId = tenantId 
            };

            // Use Add directly or Attach if tracking issues arise (InMemory is usually fine)
            // Check if item exists first to avoid key duplication if context reused (Singleton DbContext? No, scoped)
            // But InMemory database name "WarehouseTest" is shared across tests if configured in factory!
            // Factory configures "WarehouseTest".
            // So DB persists across tests unless verified/cleared.
            // XUnit Collection Fixture shares the factory -> shares the InMemory DB instance name.
            
            if (!await warehouseContext.StockItems.AnyAsync(i => i.Id == 999))
            {
                warehouseContext.StockItems.Add(stockItem);
                warehouseContext.StockHoldings.Add(stockHolding);
                await warehouseContext.SaveChangesAsync();
            }
            else
            {
                // Reset stock if exists
                var holding = await warehouseContext.StockHoldings.FirstAsync(h => h.StockItemId == 999);
                holding.QuantityOnHand = 10;
                await warehouseContext.SaveChangesAsync();
            }
        }

        var command = new CreateOrderCommand(
            CustomerId: Guid.NewGuid(),
            Lines: new List<CreateOrderRequestLine>
            {
                new CreateOrderRequestLine(999, "Test Item", 5, 10.0m)
            }
        );

        // Act
        // Ensure client sends tenant header
        Client.DefaultRequestHeaders.Remove("X-Tenant-Id");
        Client.DefaultRequestHeaders.Add("X-Tenant-Id", tenantId.ToString());

        var response = await Client.PostAsJsonAsync("/api/orders", command);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var orderId = await response.Content.ReadFromJsonAsync<Guid>();

        // Wait for Async Processing (Sales -> Warehouse -> Sales)
        // With In-Memory Bus, it's fast but still async.
        // We'll poll.
        
        using (var scope = Services.CreateScope())
        {
            var salesContext = scope.ServiceProvider.GetRequiredService<SalesDbContext>();
            var warehouseContext = scope.ServiceProvider.GetRequiredService<WarehouseDbContext>();

            // Poll for Order Status 'Confirmed'
            // Retry logic
            for (int i = 0; i < 20; i++) // 2 seconds max
            {
                var order = await salesContext.Orders.AsNoTracking().FirstOrDefaultAsync(o => o.Id == orderId); // AsNoTracking to get fresh data? Or reload.
                if (order?.Status == "Confirmed") break;
                await Task.Delay(100);
            }

            var confirmedOrder = await salesContext.Orders.FindAsync(orderId);
            confirmedOrder.Should().NotBeNull();
            confirmedOrder!.Status.Should().Be("Confirmed", "Warehouse should have reserved stock and confirmed order.");

            // Poll for Stock Update
             for (int i = 0; i < 20; i++) 
            {
                 var h = await warehouseContext.StockHoldings.AsNoTracking().FirstOrDefaultAsync(x => x.StockItemId == 999);
                 if (h?.QuantityOnHand == 5) break; 
                 await Task.Delay(100);
            }
            
            var holding = await warehouseContext.StockHoldings.FirstOrDefaultAsync(h => h.StockItemId == 999);
            holding.Should().NotBeNull();
            holding!.QuantityOnHand.Should().Be(5);
        }
    }
}
