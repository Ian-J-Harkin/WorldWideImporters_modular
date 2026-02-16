using MediatR;
using MassTransit;
using WWI_ModularKit.Modules.Sales.Persistence;
using WWI_ModularKit.Modules.Sales.Entities;
using WWI_ModularKit.Modules.Sales.Contracts.Events;

using WWI_ModularKit.BuildingBlocks.Abstractions;

namespace WWI_ModularKit.Modules.Sales.Features.Orders.CreateOrder;

public class CreateOrderHandler(
    SalesDbContext dbContext, 
    IPublishEndpoint publishEndpoint,
    ITenantProvider tenantProvider) 
    : IRequestHandler<CreateOrderCommand, Guid>
{
    public async Task<Guid> Handle(CreateOrderCommand request, CancellationToken cancellationToken)
    {
        using var transaction = await dbContext.Database.BeginTransactionAsync(cancellationToken);

        var order = new Order
        {
            CustomerId = request.CustomerId,
            OrderDate = DateTime.UtcNow,
            Lines = request.Lines.Select(l => new OrderLine
            {
                StockItemId = l.StockItemId,
                Description = l.Description,
                Quantity = l.Quantity,
                UnitPrice = l.UnitPrice
            }).ToList()
        };

        dbContext.Orders.Add(order);
        
        // We publish the event before SaveChanges so it's persisted to the Outbox within the same transaction.
        // We pull the TenantId from the provider to ensure the integration event is correctly populated.
        var tenantId = tenantProvider.GetTenantId();
        await publishEndpoint.Publish(new OrderCreatedIntegrationEvent(order.Id, tenantId), cancellationToken);

        await dbContext.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);

        return order.Id;
    }
}
