using MediatR;
using Microsoft.EntityFrameworkCore;
using WWI_ModularKit.Modules.Sales.Persistence;

namespace WWI_ModularKit.Modules.Sales.Features.Orders.GetOrders;

public class GetOrdersHandler(SalesDbContext dbContext) 
    : IRequestHandler<GetOrdersQuery, List<OrderResponse>>
{
    public async Task<List<OrderResponse>> Handle(GetOrdersQuery request, CancellationToken cancellationToken)
    {
        // The BaseDbContext automatically applies the tenant filter
        var orders = await dbContext.Orders
            .Include(o => o.Customer)
            .Include(o => o.Lines)
            .ToListAsync(cancellationToken);

        return orders.Select(o => new OrderResponse(
            o.Id,
            o.OrderDate,
            o.ExpectedDeliveryDate,
            o.CustomerPurchaseOrderNumber,
            o.IsUndeliverable,
            o.PickingCompletedWhen,
            o.CustomerId,
            o.Customer?.Name ?? "Unknown",
            o.Lines.Select(l => new OrderLineResponse(
                l.Id,
                l.StockItemId,
                l.Description,
                l.Quantity,
                l.UnitPrice
            )).ToList()
        )).ToList();
    }
}
