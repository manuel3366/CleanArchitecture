var builder = DistributedApplication.CreateBuilder(args);

builder.AddProject<Projects.CleanArchitecture_Api>("CleanArchitecture-Api")
    .WithEnvironment("ASPIRE_ENABLED", "true")
    .WithOtlpExporter()
    .WithHttpHealthCheck("/health");

builder.Build().Run();