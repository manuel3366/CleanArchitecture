using System;
using Aikido.Zen.DotNetCore;
using CleanArchitecture.Api.BackgroundServices;
using CleanArchitecture.Api.Extensions;
using CleanArchitecture.Application.Extensions;
using CleanArchitecture.Application.gRPC;
using CleanArchitecture.Domain.Extensions;
using CleanArchitecture.Infrastructure.Database;
using CleanArchitecture.Infrastructure.Extensions;
using CleanArchitecture.ServiceDefaults;
using HealthChecks.ApplicationStatus.DependencyInjection;
using HealthChecks.UI.Client;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

var builder = WebApplication.CreateBuilder(args);
var databaseName = builder.Configuration["DatabaseName"] ?? "clean-architecture";

builder.AddServiceDefaults();

builder.Services.AddControllers();
builder.Services.AddGrpc();
builder.Services.AddGrpcReflection();
builder.Services.AddEndpointsApiExplorer();

if (builder.Environment.IsProduction())
{
    builder.Services.AddZenFirewall();
}

builder.Services
    .AddHealthChecks()
    .AddDbContextCheck<ApplicationDbContext>()
    .AddApplicationStatus();

builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    options.UseLazyLoadingProxies();
    options.UseInMemoryDatabase(databaseName);
});

builder.Services.AddSwagger();
builder.Services.AddAuth(builder.Configuration);
builder.Services.AddInfrastructure(databaseName);
builder.Services.AddQueryHandlers();
builder.Services.AddServices();
builder.Services.AddSortProviders();
builder.Services.AddCommandHandlers();
builder.Services.AddNotificationHandlers();
builder.Services.AddApiUser();

builder.Services.AddHostedService<SetInactiveUsersService>();

builder.Services.AddMediatR(cfg => { cfg.RegisterServicesFromAssemblies(typeof(Program).Assembly); });

builder.Services.AddLogging(x => x.AddSimpleConsole(console =>
{
    console.TimestampFormat = "[yyyy-MM-ddTHH:mm:ss.fff] ";
    console.IncludeScopes = true;
}));

builder.Services.AddDistributedMemoryCache();

var app = builder.Build();

app.MapDefaultEndpoints();

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var appDbContext = services.GetRequiredService<ApplicationDbContext>();
    var storeDbContext = services.GetRequiredService<EventStoreDbContext>();
    var domainStoreDbContext = services.GetRequiredService<DomainNotificationStoreDbContext>();

    appDbContext.EnsureDatabaseCreated();
    storeDbContext.EnsureDatabaseCreated();
    domainStoreDbContext.EnsureDatabaseCreated();
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
    app.MapGrpcReflectionService();
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

if (builder.Environment.IsProduction())
{
    app.UseZenFirewall();
}

app.MapHealthChecks("/healthz", new HealthCheckOptions
{
    ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
});
app.MapControllers();
app.MapGrpcService<UsersApiImplementation>();
app.MapGrpcService<TenantsApiImplementation>();

await app.RunAsync();

// Needed for integration tests web application factory
public partial class Program
{
}