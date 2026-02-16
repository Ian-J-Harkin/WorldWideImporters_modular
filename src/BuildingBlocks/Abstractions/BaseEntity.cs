namespace WWI_ModularKit.BuildingBlocks.Abstractions;

public abstract class BaseEntity
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid TenantId { get; set; }
}
