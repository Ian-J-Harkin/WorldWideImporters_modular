using MediatR;

namespace WWI_ModularKit.Modules.Sales.Features.Orders.CreateOrder;

public record CreateOrderCommand(Guid CustomerId, List<CreateOrderRequestLine> Lines) : IRequest<Guid>;

public record CreateOrderRequestLine(int StockItemId, string Description, int Quantity, decimal UnitPrice);
