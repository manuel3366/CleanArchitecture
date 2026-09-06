using System;
using System.Threading.Tasks;
using CleanArchitecture.Domain.Entities;
using CleanArchitecture.Infrastructure.Database;
using CleanArchitecture.IntegrationTests.Fixtures;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.DependencyInjection;

namespace CleanArchitecture.IntegrationTests.ExternalServices;

public sealed class DistributedCacheTestFixture : TestFixtureBase
{
    public Guid CreatedTenantId { get; } = Guid.NewGuid();
    
    public IDistributedCache DistributedCache { get; }

    public DistributedCacheTestFixture()
    {
        DistributedCache = Factory.Services.GetRequiredService<IDistributedCache>();
    }

    public async Task SeedTestData()
    {
        await ResetDatabaseAsync();

        using var scope = Factory.Services.CreateScope();
        using var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        context.Tenants.Add(new Tenant(
            CreatedTenantId,
            "Test Tenant"));

        await context.SaveChangesAsync();
    }
}
