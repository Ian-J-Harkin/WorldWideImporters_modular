using Xunit;
using System.Net.Http.Json;
using Microsoft.Extensions.DependencyInjection;
using WWI_ModularKit.IntegrationTests;
using WWI_ModularKit.Modules.Sales.Persistence;

namespace WWI_ModularKit.IntegrationTests;

public abstract class BaseIntegrationTest(IntegrationTestFactory factory) : IClassFixture<IntegrationTestFactory>
{
    protected readonly HttpClient Client = factory.CreateClient();
    protected readonly IServiceProvider Services = factory.Services;

    protected void SetTenant(Guid tenantId)
    {
        using var scope = Services.CreateScope();
        var provider = scope.ServiceProvider.GetRequiredService<TestTenantProvider>();
        provider.TenantId = tenantId;
    }

    protected async Task ExecuteInScope(Func<SalesDbContext, Task> action, Guid? tenantId = null)
    {
        using var scope = Services.CreateScope();
        if (tenantId.HasValue)
        {
            var provider = scope.ServiceProvider.GetRequiredService<TestTenantProvider>();
            provider.TenantId = tenantId.Value;
        }

        var context = scope.ServiceProvider.GetRequiredService<SalesDbContext>();
        await action(context);
    }
}
