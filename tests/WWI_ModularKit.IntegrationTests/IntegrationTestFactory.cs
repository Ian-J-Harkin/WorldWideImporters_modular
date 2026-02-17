using Xunit;
using System.Net;
using System.Linq;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Testcontainers.PostgreSql;
using WWI_ModularKit.Modules.Sales.Persistence;
using WWI_ModularKit.BuildingBlocks.Abstractions;

namespace WWI_ModularKit.IntegrationTests;

public class IntegrationTestFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private readonly PostgreSqlContainer _dbContainer = new PostgreSqlBuilder()
        .WithImage("postgres:latest")
        .WithDatabase("wwi_integration_tests")
        .WithUsername("postgres")
        .WithPassword("postgres")
        .Build();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            // Remove existing DbContext registrations
            var descriptors = services.Where(d => d.ServiceType.IsGenericType && 
                d.ServiceType.GetGenericTypeDefinition() == typeof(DbContextOptions<>)).ToList();
            
            foreach (var descriptor in descriptors)
            {
                services.Remove(descriptor);
            }

            // Add Test DbContext using Testcontainers Connection String
            services.AddDbContext<SalesDbContext>(options =>
                options.UseNpgsql(_dbContainer.GetConnectionString()));

            // Override ITenantProvider with TestTenantProvider (Singleton for AsyncLocal support)
            services.AddSingleton<TestTenantProvider>();
            services.AddSingleton<ITenantProvider>(sp => sp.GetRequiredService<TestTenantProvider>());

            // Add StartupFilter for Middleware
            services.AddTransient<IStartupFilter, TenantStartupFilter>();

            // Ensure schema and migrations are applied
            using var scope = services.BuildServiceProvider().CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<SalesDbContext>();
            db.Database.Migrate();
        });
    }

    public async Task InitializeAsync() => await _dbContainer.StartAsync();

    public new async Task DisposeAsync() => await _dbContainer.StopAsync();

    private class TenantStartupFilter : IStartupFilter
    {
        public Action<IApplicationBuilder> Configure(Action<IApplicationBuilder> next)
        {
            return app =>
            {
                app.UseMiddleware<TenantHeaderMiddleware>();
                next(app);
            };
        }
    }

    private class TenantHeaderMiddleware(RequestDelegate next)
    {
        public async Task InvokeAsync(HttpContext context, TestTenantProvider tenantProvider)
        {
            if (context.Request.Headers.TryGetValue("X-Tenant-Id", out var tenantIdStr))
            {
                if (Guid.TryParse(tenantIdStr, out var tenantId))
                {
                    tenantProvider.TenantId = tenantId;
                }
            }
            await next(context);
        }
    }
}
