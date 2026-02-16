namespace WWI_ModularKit.Modules.Sales.Contracts.Events;

public record OrderCreatedIntegrationEvent(Guid OrderId, Guid TenantId);
