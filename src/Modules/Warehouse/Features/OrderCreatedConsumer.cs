using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using WWI_ModularKit.Modules.Sales.Contracts.Events;
using WWI_ModularKit.Modules.Warehouse.Contracts;
using WWI_ModularKit.Modules.Warehouse.Persistence;

namespace WWI_ModularKit.Modules.Warehouse.Features;

public class OrderCreatedConsumer(WarehouseDbContext dbContext, ILogger<OrderCreatedConsumer> logger) 
    : IConsumer<OrderCreatedIntegrationEvent>
{
    public async Task Consume(ConsumeContext<OrderCreatedIntegrationEvent> context)
    {
        var message = context.Message;
        logger.LogInformation("[Warehouse] Processing Order {OrderId} for Tenant {TenantId}", message.OrderId, message.TenantId);

        // Simple single-item check for the Mock (assuming 1st line item is the target)
        // In a real app, we'd loop through lines.
        // For this mock, we only care about stock item 220 (USB Missile Launcher)
        var lineItem = message.Lines.FirstOrDefault(l => l.StockItemId == 220);

        if (lineItem == null)
        {
            logger.LogWarning("[Warehouse] Order {OrderId} has no USB Missile Launchers. Auto-confirming (Mock Logic).", message.OrderId);
            await context.Publish(new StockReservedIntegrationEvent(message.OrderId, message.TenantId));
            return;
        }

        var holding = await dbContext.StockHoldings
            .FirstOrDefaultAsync(h => h.StockItemId == 220);

        if (holding == null)
        {
             logger.LogError("[Warehouse] Stock Item 220 not found in inventory!");
             await context.Publish(new StockInsufficientIntegrationEvent(message.OrderId, message.TenantId));
             return;
        }

        if (holding.QuantityOnHand >= lineItem.Quantity)
        {
            holding.QuantityOnHand -= lineItem.Quantity;
            await dbContext.SaveChangesAsync();
            
            logger.LogInformation("[Warehouse] Stock Reserved. New Qty: {Qty}", holding.QuantityOnHand);
            await context.Publish(new StockReservedIntegrationEvent(message.OrderId, message.TenantId));
        }
        else
        {
            logger.LogWarning("[Warehouse] Insufficient Stock. Requested: {Req}, Available: {Avail}", lineItem.Quantity, holding.QuantityOnHand);
            await context.Publish(new StockInsufficientIntegrationEvent(message.OrderId, message.TenantId));
        }
    }
}
