using System;
using System.Threading;
using System.Threading.Tasks;
using CleanArchitecture.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace CleanArchitecture.IntegrationTests;

[SetUpFixture]
internal class GlobalSetupFixture
{
    private static readonly SemaphoreSlim s_databaseSeedLock = new(1, 1);

    public static async Task ResetAndSeedDatabaseAsync(
        IServiceProvider services,
        Func<Task> seedDatabase)
    {
        await s_databaseSeedLock.WaitAsync();

        try
        {
            await ResetDatabaseAsync(services);
            await seedDatabase();
        }
        finally
        {
            s_databaseSeedLock.Release();
        }
    }

    public static async Task ResetDatabaseAsync(IServiceProvider services)
    {
        await using var scope = services.CreateAsyncScope();
        var serviceProvider = scope.ServiceProvider;

        await serviceProvider.GetRequiredService<ApplicationDbContext>().Database.EnsureDeletedAsync();
        await serviceProvider.GetRequiredService<ApplicationDbContext>().Database.EnsureCreatedAsync();
    }
}