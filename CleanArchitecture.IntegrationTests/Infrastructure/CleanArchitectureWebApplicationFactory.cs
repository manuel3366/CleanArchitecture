using System;
using System.Collections.Generic;
using CleanArchitecture.IntegrationTests.Infrastructure.Auth;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using CleanArchitecture.Infrastructure.Database;

namespace CleanArchitecture.IntegrationTests.Infrastructure;

public sealed class CleanArchitectureWebApplicationFactory : WebApplicationFactory<Program>
{
    public delegate void RegisterCustomServicesHandler(
        IServiceCollection services);

    private readonly bool _addTestAuthentication;
    private readonly string _databaseName = Guid.NewGuid().ToString();
    private readonly InMemoryDatabaseRoot _databaseRoot = new();
    private readonly RegisterCustomServicesHandler? _registerCustomServicesHandler;

    public CleanArchitectureWebApplicationFactory(
        RegisterCustomServicesHandler? registerCustomServicesHandler,
        bool addTestAuthentication)
    {
        _registerCustomServicesHandler = registerCustomServicesHandler;
        _addTestAuthentication = addTestAuthentication;
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Integration");
        base.ConfigureWebHost(builder);
        builder.ConfigureAppConfiguration((_, configuration) =>
        {
            configuration.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["DatabaseName"] = _databaseName
            });
        });

        builder.ConfigureServices(services =>
        {
            services.RemoveAll<DbContextOptions<ApplicationDbContext>>();
            services.AddDbContext<ApplicationDbContext>(options =>
            {
                options.UseLazyLoadingProxies();
                options.UseInMemoryDatabase(_databaseName, _databaseRoot);
            });

            if (_addTestAuthentication)
            {
                services.AddAuthentication(options =>
                {
                    options.DefaultAuthenticateScheme = "Testing";
                    options.DefaultChallengeScheme = "Testing";
                }).AddTestAuthentication(_ => { });
            }

            _registerCustomServicesHandler?.Invoke(services);
        });
    }
}