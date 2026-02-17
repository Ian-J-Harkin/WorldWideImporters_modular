namespace WWI_ModularKit.Modules.Warehouse.Contracts;

public record StockReservedIntegrationEvent(Guid OrderId, Guid TenantId);
