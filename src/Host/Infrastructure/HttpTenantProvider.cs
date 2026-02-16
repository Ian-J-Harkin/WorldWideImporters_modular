using WWI_ModularKit.BuildingBlocks.Abstractions;
using Microsoft.AspNetCore.Http;

namespace WWI_ModularKit.Host.Infrastructure;

public class HttpTenantProvider(IHttpContextAccessor httpContextAccessor) : ITenantProvider
{
    private const string TenantHeaderName = "X-Tenant-Id";

    public Guid GetTenantId()
    {
        var context = httpContextAccessor.HttpContext;
        if (context != null && context.Request.Headers.TryGetValue(TenantHeaderName, out var tenantIdStr))
        {
            if (Guid.TryParse(tenantIdStr, out var tenantId))
            {
                return tenantId;
            }
        }

        // Production Guardrail: Require Tenant ID for all operations
        throw new BadHttpRequestException("Tenant ID is required.");
    }
}
