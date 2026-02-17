using MediatR;

namespace WWI_ModularKit.Modules.Sales.Features.Orders.GetOrders;

public record GetOrdersQuery() : IRequest<List<OrderResponse>>;

public record OrderResponse(
    Guid Id,
    DateTime OrderDate,
    DateTime ExpectedDeliveryDate,
    string? CustomerPurchaseOrderNumber,
    bool IsUndeliverable,
    DateTime? PickingCompletedWhen,
    Guid CustomerId,
    string CustomerName,
    List<OrderLineResponse> Lines);

public record OrderLineResponse(
    Guid Id,
    int StockItemId,
    string Description,
    int Quantity,
    decimal UnitPrice);
