using System;
using System.Threading.Tasks;
using CleanArchitecture.Domain.Constants;
using CleanArchitecture.Domain.Entities;
using CleanArchitecture.Domain.Enums;
using CleanArchitecture.Infrastructure.Database;
using CleanArchitecture.IntegrationTests.Infrastructure.Auth;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace CleanArchitecture.IntegrationTests.Fixtures;

public sealed class UserTestFixture : TestFixtureBase
{
    public Guid DeletedUserId { get; } = Guid.NewGuid();
    
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

            context.Users.Add(new User(
                Ids.Seed.UserId,
                Ids.Seed.TenantId,
                "admin@email.com",
                "Admin",
                "User",
                "$2a$12$Blal/uiFIJdYsCLTMUik/egLbfg3XhbnxBC6Sb5IKz2ZYhiU/MzL2",
                UserRole.Admin));

            var deletedUsed = new User(
                DeletedUserId,
                Ids.Seed.TenantId,
                "admin2@email.com",
                "Admin2",
                "User2",
                "$2a$12$Blal/uiFIJdYsCLTMUik/egLbfg3XhbnxBC6Sb5IKz2ZYhiU/MzL2",
                UserRole.User);
            deletedUsed.Delete();
            context.Users.Add(deletedUsed);

            context.Users.Add(new User(
                TestAuthenticationOptions.TestUserId,
                Ids.Seed.TenantId,
                TestAuthenticationOptions.Email,
                TestAuthenticationOptions.FirstName,
                TestAuthenticationOptions.LastName,
                TestAuthenticationOptions.Password,
                UserRole.Admin));

            await context.SaveChangesAsync();
        });
    }
}