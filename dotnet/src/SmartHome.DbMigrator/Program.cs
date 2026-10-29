using DotNetEnv;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SmartHome.Infrastructure;
using SmartHome.Infrastructure.Persistence;

internal class Program
{
    private static async Task Main(string[] args)
    {
        // Load .env without overwriting existing environment variables.
        Env.NoClobber().TraversePath().Load();

        var builder = Host.CreateApplicationBuilder(args);

        // Read the database connection string.
        var connectionString =
            builder.Configuration.GetConnectionString("SmartHome");

        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException(
                "Connection string 'SmartHome' is missing.");
        }

        // Register infrastructure services.
        builder.Services.AddInfrastructure(connectionString);

        // Build the application and apply pending migrations.
        using var host = builder.Build();
        await using var scope = host.Services.CreateAsyncScope();

        var dbContext = scope.ServiceProvider.GetRequiredService<SmartHomeDbContext>();

        await dbContext.Database.MigrateAsync();

        Console.WriteLine("Database migrations applied");
    }
}
