namespace WWI_ModularKit.Modules.Sales.Contracts.Events;

public record OrderLineDto(int StockItemId, int Quantity);

public record OrderCreatedIntegrationEvent(Guid OrderId, Guid TenantId, List<OrderLineDto> Lines);
