using System;
using System.Threading.Tasks;
using CleanArchitecture.Domain.Entities;
using CleanArchitecture.Infrastructure.Database;
using Microsoft.Extensions.DependencyInjection;

namespace CleanArchitecture.IntegrationTests.Fixtures;

public sealed class ClientTestFixture : TestFixtureBase
{
    public Guid AlanaClientId { get; } = Guid.NewGuid();
    public Guid MiloClientId { get; } = Guid.NewGuid();
    public Guid AliciaClientId { get; } = Guid.NewGuid();
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
            new Client(AlanaClientId, "Alana Rivera", "alana.rivera@example.test", "555-0101"),
            new Client(MiloClientId, "Milo Chen", "milo.chen@example.test", "555-0102"),
            new Client(AliciaClientId, "Alicia Stone", "alicia.stone@example.test", "555-0103"));

        var deletedClient = new Client(
            DeletedClientId,
            "Martin Gray",
            "gray@example.test",
            "555-0104");
        deletedClient.Delete();
        context.Clients.Add(deletedClient);

        await context.SaveChangesAsync();
    }
}
