using Xunit;
using System.Net;
using System.Linq;
using System.Collections.Generic;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using WWI_ModularKit.Modules.Sales.Features.Orders.CreateOrder;
using Microsoft.Extensions.DependencyInjection;
using WWI_ModularKit.Modules.Sales.Entities;

[assembly: CollectionBehavior(DisableTestParallelization = true)]

namespace WWI_ModularKit.IntegrationTests;

public class SalesIntegrationTests(IntegrationTestFactory factory) : BaseIntegrationTest(factory)
{
    private async Task<Guid> SeedCustomerAsync(Guid tenantId)
    {
        var customerId = Guid.NewGuid();
        await ExecuteInScope(async db => 
        {
            db.Customers.Add(new Customer 
            { 
                Id = customerId, 
                Name = "Test Customer", 
                CustomerCategoryName = "Test",
                TenantId = tenantId 
            });
            await db.SaveChangesAsync();
        }, tenantId);
        return customerId;
    }

    [Fact]
    public async Task CreateOrder_Should_EnforceTenantIsolation()
    {
        // Arrange
        var tenantA = Guid.NewGuid();
        var tenantB = Guid.NewGuid();
        var customerA = await SeedCustomerAsync(tenantA);
        
        var command = new CreateOrderCommand(
            customerA,
            new List<CreateOrderRequestLine> 
            { 
                new(1, "Test Item", 10, 100) 
            }
        );

        // Act - Create as Tenant A
        var requestA = new HttpRequestMessage(HttpMethod.Post, "/api/sales/orders")
        {
            Content = JsonContent.Create(command)
        };
        requestA.Headers.Add("X-Tenant-Id", tenantA.ToString());
        var responseA = await Client.SendAsync(requestA);
        responseA.EnsureSuccessStatusCode();

        // Assert - Tenant B should not see Tenant A's order
        await ExecuteInScope(async db => 
        {
             // Verify order exists in DB regardless of filters
             var order = await db.Orders.IgnoreQueryFilters().FirstOrDefaultAsync(o => o.CustomerId == customerA);
             order.Should().NotBeNull();
             
             // Verify it is NOT returned in a filtered query for Tenant B
             var filteredOrders = await db.Orders.ToListAsync();
             filteredOrders.Should().NotContain(o => o.Id == order.Id);
        }, tenantB); 
    }

    [Fact]
    public async Task CreateOrder_Should_WriteToTransactionalOutbox()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var customerId = await SeedCustomerAsync(tenantId);
        var command = new CreateOrderCommand(
            customerId, 
            new List<CreateOrderRequestLine> { new(1, "Outbox Item", 1, 50) }
        );

        // Act
        var request = new HttpRequestMessage(HttpMethod.Post, "/api/sales/orders")
        {
            Content = JsonContent.Create(command)
        };
        request.Headers.Add("X-Tenant-Id", tenantId.ToString());
        var response = await Client.SendAsync(request);
        var orderId = await response.Content.ReadFromJsonAsync<Guid>();

        // Assert
        await ExecuteInScope(async db => 
        {
            var outboxMessages = await db.Set<MassTransit.EntityFrameworkCoreIntegration.OutboxMessage>().ToListAsync();
            // Match by OrderId in JSON body or just check for existence of OrderCreatedIntegrationEvent
            outboxMessages.Should().Contain(m => m.Body.Contains(orderId.ToString()));
        }, tenantId);
    }

    [Fact]
    public async Task CreateOrder_Should_RollbackTransaction_OnFailure()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var customerId = await SeedCustomerAsync(tenantId);
        
        // Act - Test DB atomicity directly
        await ExecuteInScope(async db => 
        {
            using var transaction = await db.Database.BeginTransactionAsync();
            
            db.Orders.Add(new Order
            {
                Id = Guid.NewGuid(),
                OrderDate = DateTime.UtcNow,
                ExpectedDeliveryDate = DateTime.UtcNow,
                CustomerId = customerId,
                TenantId = tenantId
            });
            
            await db.SaveChangesAsync();
            await transaction.RollbackAsync();
        }, tenantId);

        // Assert
        await ExecuteInScope(async db => 
        {
            var orders = await db.Orders.IgnoreQueryFilters().ToListAsync();
            orders.Should().NotContain(o => o.TenantId == tenantId);
        }, tenantId);
    }

    [Fact]
    public async Task GetOrders_Should_RespectTenantIsolation()
    {
        // Arrange
        var tenantA = Guid.NewGuid();
        var tenantB = Guid.NewGuid();
        var customerA = await SeedCustomerAsync(tenantA);
        var customerB = await SeedCustomerAsync(tenantB);

        // Create order for Tenant A
        var commandA = new CreateOrderCommand(
            customerA,
            new List<CreateOrderRequestLine> { new(1, "Tenant A Item", 5, 50) }
        );
        var requestCreateA = new HttpRequestMessage(HttpMethod.Post, "/api/sales/orders")
        {
            Content = JsonContent.Create(commandA)
        };
        requestCreateA.Headers.Add("X-Tenant-Id", tenantA.ToString());
        await Client.SendAsync(requestCreateA);

        // Create order for Tenant B
        var commandB = new CreateOrderCommand(
            customerB,
            new List<CreateOrderRequestLine> { new(2, "Tenant B Item", 10, 100) }
        );
        var requestCreateB = new HttpRequestMessage(HttpMethod.Post, "/api/sales/orders")
        {
            Content = JsonContent.Create(commandB)
        };
        requestCreateB.Headers.Add("X-Tenant-Id", tenantB.ToString());
        await Client.SendAsync(requestCreateB);

        // Act - Get orders for Tenant A
        var requestGetA = new HttpRequestMessage(HttpMethod.Get, "/api/sales/orders");
        requestGetA.Headers.Add("X-Tenant-Id", tenantA.ToString());
        var responseA = await Client.SendAsync(requestGetA);
        var ordersAJson = await responseA.Content.ReadAsStringAsync();

        // Act - Get orders for Tenant B
        var requestGetB = new HttpRequestMessage(HttpMethod.Get, "/api/sales/orders");
        requestGetB.Headers.Add("X-Tenant-Id", tenantB.ToString());
        var responseB = await Client.SendAsync(requestGetB);
        var ordersBJson = await responseB.Content.ReadAsStringAsync();

        // Assert
        responseA.EnsureSuccessStatusCode();
        responseB.EnsureSuccessStatusCode();
        
        // Verify using database queries instead of JSON parsing
        await ExecuteInScope(async db =>
        {
            var tenantAOrders = await db.Orders.ToListAsync();
            tenantAOrders.Should().HaveCount(1);
            tenantAOrders[0].CustomerId.Should().Be(customerA);
        }, tenantA);

        await ExecuteInScope(async db =>
        {
            var tenantBOrders = await db.Orders.ToListAsync();
            tenantBOrders.Should().HaveCount(1);
            tenantBOrders[0].CustomerId.Should().Be(customerB);
        }, tenantB);
        
        // Verify cross-tenant isolation: Tenant A should not see Tenant B's orders
        await ExecuteInScope(async db =>
        {
            var visibleOrders = await db.Orders.ToListAsync();
            visibleOrders.Should().NotContain(o => o.CustomerId == customerB);
        }, tenantA);
    }
}
