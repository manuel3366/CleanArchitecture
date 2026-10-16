using System;
using System.Threading.Tasks;
using CleanArchitecture.Domain.Entities;
using CleanArchitecture.Infrastructure.Database;
using Microsoft.Extensions.DependencyInjection;

namespace CleanArchitecture.IntegrationTests.Fixtures;

public sealed class ClientTestFixture : TestFixtureBase
{
    public Guid AlphaClientId { get; } = Guid.NewGuid();
    public Guid BetaClientId { get; } = Guid.NewGuid();
    public Guid DeletedClientId { get; } = Guid.NewGuid();

    public ClientTestFixture() : base(useTestAuthentication: false)
    {
    }

    public async Task SeedTestData()
    {
        await ResetDatabaseAsync();

        using var scope = Factory.Services.CreateScope();
        using var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        context.Clients.Add(new Client(AlphaClientId, "Alpha Client", "alpha@example.test", "555-1111"));
        context.Clients.Add(new Client(BetaClientId, "Beta Client", "beta@example.test", "555-2222"));

        var deletedClient = new Client(DeletedClientId, "Deleted Client", "deleted@example.test", "555-3333");
        deletedClient.Delete();
        context.Clients.Add(deletedClient);

        await context.SaveChangesAsync();
    }
}
