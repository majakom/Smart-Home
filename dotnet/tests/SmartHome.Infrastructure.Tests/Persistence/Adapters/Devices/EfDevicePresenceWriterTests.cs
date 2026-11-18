using Microsoft.EntityFrameworkCore;
using SmartHome.Domain.Devices;
using SmartHome.Infrastructure.Persistence;
using SmartHome.Infrastructure.Persistence.Adapters.Devices;
using Testcontainers.PostgreSql;

namespace SmartHome.Infrastructure.Tests.Persistence.Adapters.Devices;

public class EfDevicePresenceWriterTests : IAsyncLifetime
{
    private readonly PostgreSqlContainer _postgres = new PostgreSqlBuilder("postgres:18").Build();

    private SmartHomeDbContext _dbContext = null!;

    public async Task InitializeAsync()
    {
        await _postgres.StartAsync();

        DbContextOptions<SmartHomeDbContext> options =
            new DbContextOptionsBuilder<SmartHomeDbContext>()
                .UseNpgsql(_postgres.GetConnectionString())
                .Options;

        _dbContext = new SmartHomeDbContext(options);

        await _dbContext.Database.MigrateAsync();
    }

    public async Task DisposeAsync()
    {
        await _dbContext.DisposeAsync();
        await _postgres.DisposeAsync();
    }

    [Fact]
    public async Task MarkAsSeenAsync_UpdatesLastSeenAtUtc_WhenDeviceExists()
    {
        // Arrange
        Device device = new("Device Name", "Device Model");
        _dbContext.Devices.Add(device);
        await _dbContext.SaveChangesAsync();

        DateTime receivedAtUtc = new(2026, 6, 6, 12, 0, 0, DateTimeKind.Utc);
        EfDevicePresenceWriter writer = new(_dbContext);

        // Act
        await writer.MarkAsSeenAsync(
            device.Id,
            receivedAtUtc,
            CancellationToken.None
        );

        // Assert
        Device? persistedDevice = await _dbContext.Devices
            .AsNoTracking()
            .SingleOrDefaultAsync(
                item => item.Id == device.Id
            );

        Assert.NotNull(persistedDevice);
        Assert.Equal(receivedAtUtc, persistedDevice.LastSeenAtUtc);
    }

    [Fact]
    public async Task MarkAsSeenAsync_DoesNotMoveLastSeenBackwards_WhenTimestampIsOlder()
    {
        // Arrange
        DateTime latestSeenAtUtc = new(2026, 6, 6, 12, 0, 0, DateTimeKind.Utc);
        Device device = new("Device Name", "Device Model");
        device.MarkAsSeen(latestSeenAtUtc);

        _dbContext.Devices.Add(device);
        await _dbContext.SaveChangesAsync();

        EfDevicePresenceWriter writer = new(_dbContext);

        DateTime earlierSeenAtUtc = new(2026, 6, 6, 11, 59, 0, DateTimeKind.Utc);

        // Act
        await writer.MarkAsSeenAsync(
            device.Id,
            earlierSeenAtUtc,
            CancellationToken.None
        );

        // Assert
        Device? persistedDevice = await _dbContext.Devices
            .AsNoTracking()
            .SingleOrDefaultAsync(item => item.Id == device.Id);

        Assert.NotNull(persistedDevice);
        Assert.Equal(latestSeenAtUtc, persistedDevice.LastSeenAtUtc);
    }

    [Fact]
    public async Task MarkAsSeenAsync_ThrowsInvalidOperationException_WhenDeviceDoesNotExist()
    {
        // Arrange
        EfDevicePresenceWriter writer = new(_dbContext);

        // Act
        Func<Task> act = () => writer.MarkAsSeenAsync(
            DeviceId.New(),
            new DateTime(2026, 6, 6, 12, 0, 0, DateTimeKind.Utc),
            CancellationToken.None
        );

        // Assert
        await Assert.ThrowsAsync<InvalidOperationException>(act);
    }
}
