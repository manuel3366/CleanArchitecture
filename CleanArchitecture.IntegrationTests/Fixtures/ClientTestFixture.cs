using System;
using System.Threading.Tasks;
using CleanArchitecture.Domain.Entities;
using CleanArchitecture.Infrastructure.Database;
using Microsoft.Extensions.DependencyInjection;

namespace CleanArchitecture.IntegrationTests.Fixtures;

public sealed class ClientTestFixture : TestFixtureBase
{
    public Guid ActiveClientId { get; } = Guid.NewGuid();
    public Guid InactiveClientId { get; } = Guid.NewGuid();
    public Guid AnotherActiveClientId { get; } = Guid.NewGuid();

    public async Task SeedTestData()
    {
        await ResetDatabaseAsync();

        using var scope = Factory.Services.CreateScope();
        using var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        context.Clients.Add(new Client(
            ActiveClientId,
            "Ana",
            "García",
            "12345678",
            "ana.garcia@test.com",
            "Activo",
            new DateTimeOffset(2024, 01, 15, 10, 0, 0, TimeSpan.Zero),
            "04145556677"));

        context.Clients.Add(new Client(
            InactiveClientId,
            "Luis",
            "Pérez",
            "87654321",
            "luis.perez@test.com",
            "Inactivo",
            new DateTimeOffset(2024, 02, 10, 10, 0, 0, TimeSpan.Zero),
            "04145556678"));

        context.Clients.Add(new Client(
            AnotherActiveClientId,
            "Maria",
            "Lopez",
            "11223344",
            "maria.lopez@test.com",
            "Activo",
            new DateTimeOffset(2024, 03, 20, 10, 0, 0, TimeSpan.Zero),
            "04145556679"));

        await context.SaveChangesAsync();
    }
}
