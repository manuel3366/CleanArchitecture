using System;
using System.Threading.Tasks;
using CleanArchitecture.Domain.Entities;
using CleanArchitecture.Infrastructure.Database;
using CleanArchitecture.IntegrationTests.Fixtures;
using Microsoft.Extensions.DependencyInjection;

namespace CleanArchitecture.IntegrationTests.Fixtures;

public sealed class ClientTestFixture : TestFixtureBase
{
    public Guid FirstClientId { get; } = Guid.NewGuid();
    public Guid SecondClientId { get; } = Guid.NewGuid();
    public Guid ThirdClientId { get; } = Guid.NewGuid();
    public Guid DeletedClientId { get; } = Guid.NewGuid();

    public ClientTestFixture() : base(useTestAuthentication: false)
    {
    }

    public async Task SeedTestData()
    {
        await ResetDatabaseAsync();

        await using var scope = Factory.Services.CreateAsyncScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        context.Clients.AddRange(
            new Client(FirstClientId, "Amina Smith", "amina@example.com", "555-0101"),
            new Client(SecondClientId, "Bruno Jones", "bruno@example.com", "555-0102"),
            new Client(ThirdClientId, "Charlie Brown", "charlie@demo.test", "777-1000"));

        var deletedClient = new Client(
            DeletedClientId,
            "Deleted Client",
            "deleted@example.com",
            "999-9999");
        deletedClient.Delete();
        context.Clients.Add(deletedClient);

        await context.SaveChangesAsync();
    }
}
