using System;
using System.Threading.Tasks;
using CleanArchitecture.Domain.Entities;
using CleanArchitecture.Infrastructure.Database;
using Microsoft.Extensions.DependencyInjection;

namespace CleanArchitecture.IntegrationTests.Fixtures;

public sealed class ClientTestFixture : TestFixtureBase
{
    public Guid AlphaClientId { get; } = Guid.NewGuid();
    public Guid BravoClientId { get; } = Guid.NewGuid();
    public Guid CharlieClientId { get; } = Guid.NewGuid();
    public Guid DeletedClientId { get; } = Guid.NewGuid();

    public ClientTestFixture() : base(useTestAuthentication: false)
    {
    }

    public async Task SeedTestData()
    {
        await ResetDatabaseAsync();

        using var scope = Factory.Services.CreateScope();
        using var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        context.Clients.AddRange(
            new Client(AlphaClientId, "Alpha", "alpha@example.com", "555-0100"),
            new Client(BravoClientId, "Bravo", "bravo@example.com", "555-0200"),
            new Client(CharlieClientId, "Charlie", "charlie@example.com", "555-0300"));

        var deletedClient = new Client(DeletedClientId, "Deleted", "deleted@example.com", "555-0400");
        deletedClient.Delete();
        context.Clients.Add(deletedClient);

        await context.SaveChangesAsync();
    }
}
