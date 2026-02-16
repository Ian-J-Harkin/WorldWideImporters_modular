using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using WWI_ModularKit.BuildingBlocks.Abstractions;

namespace WWI_ModularKit.Modules.Sales.Persistence;

public class SalesDbContextFactory : IDesignTimeDbContextFactory<SalesDbContext>
{
    public SalesDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<SalesDbContext>();
        optionsBuilder.UseNpgsql("Host=localhost;Database=wwi_modular_monolith;Username=wwi_admin;Password=wwi_password");

        return new SalesDbContext(optionsBuilder.Options, new DesignTimeTenantProvider());
    }

    private class DesignTimeTenantProvider : ITenantProvider
    {
        public Guid GetTenantId() => Guid.Empty;
    }
}
