using System;
using System.Threading.Tasks;
using CleanArchitecture.Domain.Constants;
using CleanArchitecture.Domain.Entities;
using CleanArchitecture.Domain.Enums;
using CleanArchitecture.Infrastructure.Database;
using Grpc.Net.Client;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace CleanArchitecture.IntegrationTests.Fixtures.gRPC;

public sealed class GetUsersByIdsTestFixture : TestFixtureBase
{
    public GrpcChannel GrpcChannel { get; }
    public Guid CreatedUserId { get; } = Guid.NewGuid();

    public GetUsersByIdsTestFixture()
    {
        GrpcChannel = GrpcChannel.ForAddress("http://localhost", new GrpcChannelOptions
        {
            HttpHandler = Factory.Server.CreateHandler()
        });
    }

    public async Task SeedTestData()
    {
        await GlobalSetupFixture.ResetAndSeedDatabaseAsync(Factory.Services, async () =>
        {

            using var scope = Factory.Services.CreateScope();
            using var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            context.Users.RemoveRange(context.Users.IgnoreQueryFilters());
            context.Tenants.RemoveRange(context.Tenants.IgnoreQueryFilters());
            await context.SaveChangesAsync();

            context.Tenants.Add(new Tenant(
                Ids.Seed.TenantId,
                "Admin Tenant"));

            var user = CreateUser();
            user.Delete();

            context.Users.Add(user);
            await context.SaveChangesAsync();
        });
    }

    public User CreateUser()
    {
        return new User(
            CreatedUserId,
            Ids.Seed.TenantId,
            "user@user.de",
            "User First Name",
            "User Last Name",
            "User Password",
            UserRole.User);
    }
}