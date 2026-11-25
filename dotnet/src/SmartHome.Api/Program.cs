using DotNetEnv;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Scalar.AspNetCore;
using SmartHome.Api.ErrorHandling;
using SmartHome.Api.Features.Devices;
using SmartHome.Api.Features.Sensors;
using SmartHome.Application;
using SmartHome.Infrastructure;
using SmartHome.Infrastructure.Persistence;

public partial class Program
{
    private static async Task Main(string[] args)
    {
        Env.NoClobber().TraversePath().Load();
        
        WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

        // OpenAPI
        builder.Services.AddOpenApi();

        string connectionString =
            builder.Configuration.GetConnectionString("SmartHome")
            ?? throw new InvalidOperationException(
                "Connection string 'SmartHome' is not configured."
            );

        builder.Services.AddApplication();
        builder.Services.AddInfrastructure(connectionString);

        // Health checks
        builder.Services
            .AddHealthChecks()
            .AddDbContextCheck<SmartHomeDbContext>(
                name: "postgres",
                tags: ["ready"]
            );

        // Error handling
        builder.Services.AddProblemDetails();
        builder.Services.AddExceptionHandler<GlobalExceptionHandler>();

        WebApplication app = builder.Build();

        // Development tools
        if (app.Environment.IsDevelopment())
        {
            app.MapOpenApi();
            app.MapScalarApiReference();
        }

        // Exception handling middleware
        app.UseExceptionHandler();

        app.UseHttpsRedirection();

        // Health checks
        app.MapHealthChecks("/health/live", new HealthCheckOptions
        {
            Predicate = _ => false
        })
        .WithName("LivenessCheck")
        .WithSummary("Checks whether the API is running");

        app.MapHealthChecks("/health/ready", new HealthCheckOptions
        {
            Predicate = check => check.Tags.Contains("ready")
        })
        .WithName("ReadinessCheck")
        .WithSummary("Checks whether the API and its dependencies are ready");

        app.MapSensorEndpoints();
        app.MapDeviceEndpoints();

        await app.RunAsync();
    }
}
