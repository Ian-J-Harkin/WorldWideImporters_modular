using MassTransit;
using WWI_ModularKit.Modules.Sales.Contracts.Events;

namespace WWI_ModularKit.Host.Infrastructure.Mocks;

public class MockWarehouseConsumer : IConsumer<OrderCreatedIntegrationEvent>
{
    private readonly ILogger<MockWarehouseConsumer> _logger;

    public MockWarehouseConsumer(ILogger<MockWarehouseConsumer> logger)
    {
        _logger = logger;
    }

    public Task Consume(ConsumeContext<OrderCreatedIntegrationEvent> context)
    {
        _logger.LogInformation("[MOCK] Warehouse received order {OrderId} for Tenant {TenantId}", context.Message.OrderId, context.Message.TenantId);
        return Task.CompletedTask;
    }
}
