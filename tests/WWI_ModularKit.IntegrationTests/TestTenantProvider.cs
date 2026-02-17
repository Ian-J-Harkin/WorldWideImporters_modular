using WWI_ModularKit.BuildingBlocks.Abstractions;

namespace WWI_ModularKit.IntegrationTests;

public class TestTenantProvider : ITenantProvider
{
    public Guid TenantId { get; set; } = Guid.NewGuid();

    public Guid GetTenantId() 
    {
        if (TenantId == Guid.Empty) return Guid.Empty; // Fallback
        return TenantId;
    }
}
