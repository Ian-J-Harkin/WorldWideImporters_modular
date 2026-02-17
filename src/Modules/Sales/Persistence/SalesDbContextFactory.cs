using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using WWI_ModularKit.BuildingBlocks.Abstractions;

namespace WWI_ModularKit.Modules.Sales.Persistence;

public class SalesDbContextFactory : IDesignTimeDbContextFactory<SalesDbContext>
{
    public SalesDbContext CreateDbContext(string[] args)
    {
        // Path to the Host project appsettings.json
        var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Development";
        var basePath = Path.Combine(Directory.GetCurrentDirectory(), "src", "Host");
        
        // Handle case where we might already be in src/Host or another subdir
        if (!Directory.Exists(basePath))
        {
            basePath = Directory.GetCurrentDirectory(); 
        }

        var configuration = new ConfigurationBuilder()
            .SetBasePath(basePath)
            .AddJsonFile("appsettings.json", optional: false)
            .AddJsonFile($"appsettings.{environment}.json", optional: true)
            .AddEnvironmentVariables()
            .Build();

        var optionsBuilder = new DbContextOptionsBuilder<SalesDbContext>();
        var connectionString = configuration.GetConnectionString("DefaultConnection");
        
        optionsBuilder.UseNpgsql(connectionString);

        return new SalesDbContext(optionsBuilder.Options, new DesignTimeTenantProvider());
    }

    private class DesignTimeTenantProvider : ITenantProvider
    {
        public Guid GetTenantId() => Guid.Empty;
    }
}
