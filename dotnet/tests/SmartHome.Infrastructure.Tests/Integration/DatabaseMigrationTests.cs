using Microsoft.EntityFrameworkCore;
using SmartHome.Infrastructure.Persistence;
using Testcontainers.PostgreSql;

namespace SmartHome.Infrastructure.Tests.Persistence.Integration;

public class DatabaseMigrationTests : IAsyncLifetime
{
    private readonly PostgreSqlContainer _postgres = new PostgreSqlBuilder("postgres:18").Build();

    public async Task InitializeAsync()
    {
        await _postgres.StartAsync();
    }    

    public async Task DisposeAsync()
    {
        await _postgres.DisposeAsync();
    }

    [Fact]
    public async Task MigrateAsync_AppliesAllMigrations_WhenDatabaseIsEmpty()
    {
        // Arrange
        DbContextOptions<SmartHomeDbContext> options = 
            new DbContextOptionsBuilder<SmartHomeDbContext>()
                .UseNpgsql(_postgres.GetConnectionString())
                .Options;

        await using SmartHomeDbContext dbContext = new(options);

        // Act
        await dbContext.Database.MigrateAsync();

        IEnumerable<string> appliedMigrations = await dbContext.Database.GetAppliedMigrationsAsync();
        IEnumerable<string> pendingMigrations = await dbContext.Database.GetPendingMigrationsAsync();
        IEnumerable<string> availableMigrations = dbContext.Database.GetMigrations();

        await dbContext.Database.OpenConnectionAsync();

        await using var command = 
            dbContext.Database.GetDbConnection().CreateCommand();

        command.CommandText = """
            SELECT table_name
            FROM information_schema.tables
            WHERE table_schema = current_schema()
              AND table_type = 'BASE TABLE'
            """;

        await using var reader = await command.ExecuteReaderAsync();

        HashSet<string> tables = new(StringComparer.Ordinal);

        while (await reader.ReadAsync())
        {
            tables.Add(reader.GetString(0));
        }

        // Assert
        Assert.NotEmpty(availableMigrations);
        Assert.Equal(
            availableMigrations.OrderBy(migration => migration),
            appliedMigrations.OrderBy(migration => migration)
        );
        Assert.Empty(pendingMigrations);

        Assert.Contains("devices", tables);
        Assert.Contains("sensors", tables);
        Assert.Contains("climate_measurements", tables);
    }
}
