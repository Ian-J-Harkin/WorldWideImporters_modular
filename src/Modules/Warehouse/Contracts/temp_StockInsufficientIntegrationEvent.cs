namespace WWI_ModularKit.Modules.Warehouse.Contracts;

public record StockInsufficientIntegrationEvent(Guid OrderId, Guid TenantId);
