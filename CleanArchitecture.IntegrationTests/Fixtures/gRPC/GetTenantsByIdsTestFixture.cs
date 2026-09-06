using System;
using System.Threading.Tasks;
using CleanArchitecture.Domain.Entities;
using CleanArchitecture.Infrastructure.Database;
using Grpc.Net.Client;
using Microsoft.Extensions.DependencyInjection;

namespace CleanArchitecture.IntegrationTests.Fixtures.gRPC;

public sealed class GetTenantsByIdsTestFixture : TestFixtureBase
{
    public GrpcChannel GrpcChannel { get; }
    public Guid CreatedTenantId { get; } = Guid.NewGuid();

    public GetTenantsByIdsTestFixture()
    {
        GrpcChannel = GrpcChannel.ForAddress("http://localhost", new GrpcChannelOptions
        {
            HttpHandler = Factory.Server.CreateHandler()
        });
    }

    public async Task SeedTestData()
    {
        await ResetDatabaseAsync();

        using var scope = Factory.Services.CreateScope();
        using var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        var tenant = CreateTenant();
        tenant.Delete();

        context.Tenants.Add(tenant);
        await context.SaveChangesAsync();
    }

    public Tenant CreateTenant()
    {
        return new Tenant(
            CreatedTenantId,
            "Test Tenant");
    }
}