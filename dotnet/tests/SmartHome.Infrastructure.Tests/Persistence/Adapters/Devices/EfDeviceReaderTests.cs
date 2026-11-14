using Microsoft.EntityFrameworkCore;
using SmartHome.Domain.Devices;
using SmartHome.Infrastructure.Persistence;
using SmartHome.Infrastructure.Persistence.Adapters.Devices;
using Testcontainers.PostgreSql;

namespace SmartHome.Infrastructure.Tests.Persistence.Adapters.Devices;

public class EfDeviceReaderTests : IAsyncLifetime
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
    public async Task GetByIdAsync_ReturnsDevice_WhenDeviceExists()
    {
        // Arrange
        Device expectedDevice = new Device("Device Name", "Device Model");

        _dbContext.Devices.Add(expectedDevice);
        await _dbContext.SaveChangesAsync();

        EfDeviceReader reader = new(_dbContext);

        // Act
        Device? result = await reader.GetByIdAsync(expectedDevice.Id, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(expectedDevice.Id, result.Id);
        Assert.Equal(expectedDevice.Name, result.Name);
        Assert.Equal(expectedDevice.Model, result.Model);
        Assert.Equal(
            EntityState.Detached,
            _dbContext.Entry(result).State
        );
    }

    [Fact]
    public async Task GetByIdAsync_ReturnsNull_WhenDeviceDoesNotExist()
    {
        // Arrange
        EfDeviceReader reader = new(_dbContext);

        DeviceId deviceId = DeviceId.New();

        // Act
        Device? result = await reader.GetByIdAsync(
            deviceId,
            CancellationToken.None
        );

        // Assert
        Assert.Null(result);
    }
}
