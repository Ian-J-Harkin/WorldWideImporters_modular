using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using WWI_ModularKit.BuildingBlocks.Abstractions;
using WWI_ModularKit.Modules.Sales.Contracts.Events;
using WWI_ModularKit.Modules.Warehouse.Contracts;
using WWI_ModularKit.Modules.Warehouse.Entities;
using WWI_ModularKit.Modules.Warehouse.Features;
using WWI_ModularKit.Modules.Warehouse.Persistence;
using Xunit;

namespace WWI_ModularKit.UnitTests.Modules.Warehouse.Features;

public class OrderCreatedConsumerTests
{
    private WarehouseDbContext _dbContext;
    private readonly Mock<ILogger<OrderCreatedConsumer>> _mockLogger;
    private readonly Mock<ITenantProvider> _mockTenantProvider;
    private readonly OrderCreatedConsumer _consumer;

    public OrderCreatedConsumerTests()
    {
        var options = new DbContextOptionsBuilder<WarehouseDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _mockTenantProvider = new Mock<ITenantProvider>();
        // Mock Tenant Provider usually not needed for Consume logic unless it checks tenant context,
        // but it is needed for BaseDbContext constructor.
        
        _dbContext = new WarehouseDbContext(options, _mockTenantProvider.Object);
        _mockLogger = new Mock<ILogger<OrderCreatedConsumer>>();

        _consumer = new OrderCreatedConsumer(_dbContext, _mockLogger.Object);
    }

    [Fact]
    public async Task Consume_ShouldReserveStock_WhenStockIsSufficient()
    {
        // Arrange
        var stockItemId = 220;
        var initialQty = 10;
        var orderQty = 5;
        var tenantId = Guid.NewGuid();

        _dbContext.StockHoldings.Add(new StockHolding 
        { 
            StockItemId = stockItemId, 
            QuantityOnHand = initialQty, 
            TenantId = tenantId 
        });
        await _dbContext.SaveChangesAsync();

        var message = new OrderCreatedIntegrationEvent(
            Guid.NewGuid(),
            tenantId,
            new List<OrderLineDto> { new OrderLineDto(stockItemId, orderQty) }
        );

        var mockContext = new Mock<ConsumeContext<OrderCreatedIntegrationEvent>>();
        mockContext.Setup(x => x.Message).Returns(message);
        mockContext.Setup(x => x.Publish(It.IsAny<object>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        await _consumer.Consume(mockContext.Object);

        // Assert
        // Reload entity to get fresh state
        var holding = await _dbContext.StockHoldings.AsNoTracking().FirstOrDefaultAsync(h => h.StockItemId == stockItemId);
        Assert.NotNull(holding);
        Assert.Equal(initialQty - orderQty, holding.QuantityOnHand);

        mockContext.Verify(x => x.Publish(
            It.Is<StockReservedIntegrationEvent>(e => e.OrderId == message.OrderId), 
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Consume_ShouldFail_WhenStockIsInsufficient()
    {
        // Arrange
        var stockItemId = 220;
        var initialQty = 2; // Less than demanded
        var orderQty = 5;
        var tenantId = Guid.NewGuid();

        _dbContext.StockHoldings.Add(new StockHolding 
        { 
            StockItemId = stockItemId, 
            QuantityOnHand = initialQty, 
            TenantId = tenantId 
        });
        await _dbContext.SaveChangesAsync();

        var message = new OrderCreatedIntegrationEvent(
            Guid.NewGuid(),
            tenantId,
            new List<OrderLineDto> { new OrderLineDto(stockItemId, orderQty) }
        );

        var mockContext = new Mock<ConsumeContext<OrderCreatedIntegrationEvent>>();
        mockContext.Setup(x => x.Message).Returns(message);
        mockContext.Setup(x => x.Publish(It.IsAny<object>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        await _consumer.Consume(mockContext.Object);

        // Assert
        var holding = await _dbContext.StockHoldings.AsNoTracking().FirstOrDefaultAsync(h => h.StockItemId == stockItemId);
        Assert.NotNull(holding);
        Assert.Equal(initialQty, holding.QuantityOnHand); // Should NOT change

        mockContext.Verify(x => x.Publish(
            It.Is<StockInsufficientIntegrationEvent>(e => e.OrderId == message.OrderId), 
            It.IsAny<CancellationToken>()), Times.Once);
    }
}
