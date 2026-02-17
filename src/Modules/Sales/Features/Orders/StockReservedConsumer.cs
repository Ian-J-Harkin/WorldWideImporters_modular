using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using WWI_ModularKit.Modules.Sales.Persistence;
using WWI_ModularKit.Modules.Warehouse.Contracts;

namespace WWI_ModularKit.Modules.Sales.Features.Orders;

public class StockReservedConsumer(SalesDbContext dbContext, ILogger<StockReservedConsumer> logger) 
    : IConsumer<StockReservedIntegrationEvent>
{
    public async Task Consume(ConsumeContext<StockReservedIntegrationEvent> context)
    {
        var message = context.Message;
        logger.LogInformation("[Sales] Stock Reserved for Order {OrderId}. Updating Status...", message.OrderId);

        var order = await dbContext.Orders.FirstOrDefaultAsync(o => o.Id == message.OrderId);
        
        if (order == null)
        {
            logger.LogWarning("[Sales] Order {OrderId} not found to confirm!", message.OrderId);
            return;
        }

        order.Status = "Confirmed";
        await dbContext.SaveChangesAsync();
        
        logger.LogInformation("[Sales] Order {OrderId} Status -> Confirmed ✅", message.OrderId);
    }
}
