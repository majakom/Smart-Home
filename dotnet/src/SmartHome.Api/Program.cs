using DotNetEnv;
using Scalar.AspNetCore;
using SmartHome.Api.ErrorHandling;
using SmartHome.Api.Features.Devices;
using SmartHome.Application;
using SmartHome.Infrastructure;

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

        app.MapGet("/health", () => Results.Ok(new
        {
            Status = "Healthy"
        }))
        .WithName("HealthCheck")
        .WithSummary("Checks API health");

        app.MapDeviceEndpoints();

        await app.RunAsync();
    }
}
