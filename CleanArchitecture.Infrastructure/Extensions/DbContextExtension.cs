using Microsoft.EntityFrameworkCore;

namespace CleanArchitecture.Infrastructure.Extensions;

public static class DbContextExtension
{
    public static void EnsureDatabaseCreated(this DbContext context)
    {
        context.Database.EnsureCreated();
    }
}