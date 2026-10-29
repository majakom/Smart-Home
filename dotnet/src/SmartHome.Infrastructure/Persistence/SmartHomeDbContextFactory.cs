using DotNetEnv;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace SmartHome.Infrastructure.Persistence;

public class SmartHomeDbContextFactory
    : IDesignTimeDbContextFactory<SmartHomeDbContext>
{
    public SmartHomeDbContext CreateDbContext(string[] args)
    {
        Env.TraversePath().Load();

        var connectionString =
            Environment.GetEnvironmentVariable("ConnectionStrings__SmartHome");

        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException(
                "Set ConnectionStrings__SmartHome in the root .env file.");
        }

        var options = new DbContextOptionsBuilder<SmartHomeDbContext>()
            .UseNpgsql(connectionString)
            .Options;

        return new SmartHomeDbContext(options);
    }
}
