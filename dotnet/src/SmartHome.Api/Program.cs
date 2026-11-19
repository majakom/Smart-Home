using DotNetEnv;
using Scalar.AspNetCore;
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

        WebApplication app = builder.Build();

        // Development tools
        if (app.Environment.IsDevelopment())
        {
            app.MapOpenApi();
            app.MapScalarApiReference();
        }



        app.UseHttpsRedirection();

        app.MapGet("/health", () => Results.Ok(new
        {
            Status = "Healthy"
        }))
        .WithName("HealthCheck")
        .WithSummary("Checks API health");

        await app.RunAsync();
    }
}
