using WWI_ModularKit.BuildingBlocks.Abstractions;
using Microsoft.AspNetCore.Http;

namespace WWI_ModularKit.Host.Infrastructure;

public class HttpTenantProvider(IHttpContextAccessor httpContextAccessor) : ITenantProvider
{
    private const string TenantHeaderName = "X-Tenant-Id";

    public Guid GetTenantId()
    {
        var context = httpContextAccessor.HttpContext;
        if (context == null) 
            throw new BadHttpRequestException("No HTTP context available.");

        if (!context.Request.Headers.TryGetValue(TenantHeaderName, out var tenantIdStr) || string.IsNullOrWhiteSpace(tenantIdStr))
        {
            throw new BadHttpRequestException($"Tenant ID header '{TenantHeaderName}' is required.");
        }

        if (!Guid.TryParse(tenantIdStr, out var tenantId))
        {
            throw new BadHttpRequestException($"Invalid Tenant ID format: '{tenantIdStr}'. Must be a valid Guid.");
        }

        return tenantId;
    }
}
