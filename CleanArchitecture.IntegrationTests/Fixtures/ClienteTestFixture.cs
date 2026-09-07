using System;
using System.Threading.Tasks;
using CleanArchitecture.Domain.Entities;
using CleanArchitecture.Infrastructure.Database;
using Microsoft.Extensions.DependencyInjection;

namespace CleanArchitecture.IntegrationTests.Fixtures;

public sealed class ClienteTestFixture : TestFixtureBase
{
    public Guid ActiveClienteId { get; } = Guid.NewGuid();
    public Guid InactiveClienteId { get; } = Guid.NewGuid();
    public Guid LastClienteId { get; } = Guid.NewGuid();

    public async Task SeedTestData()
    {
        await ResetDatabaseAsync();

        using var scope = Factory.Services.CreateScope();
        using var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        var activeRegistration = new DateTimeOffset(2026, 1, 15, 0, 0, 0, TimeSpan.Zero);
        var inactiveRegistration = new DateTimeOffset(2026, 2, 20, 0, 0, 0, TimeSpan.Zero);
        var lastRegistration = new DateTimeOffset(2026, 3, 4, 0, 0, 0, TimeSpan.Zero);

        context.Clientes.Add(new Cliente(
            ActiveClienteId,
            "Ana",
            "García",
            "12345678",
            "ana.garcia@correo.com",
            "+584141234567",
            "Activo",
            activeRegistration,
            activeRegistration));

        context.Clientes.Add(new Cliente(
            InactiveClienteId,
            "Pedro",
            "Pérez",
            "87654321",
            "pedro.perez@correo.com",
            "+584149876543",
            "Inactivo",
            inactiveRegistration,
            inactiveRegistration));

        context.Clientes.Add(new Cliente(
            LastClienteId,
            "María",
            "López",
            "11223344",
            "maria.lopez@correo.com",
            "+584143334455",
            "Activo",
            lastRegistration,
            lastRegistration));

        await context.SaveChangesAsync();
    }
}
