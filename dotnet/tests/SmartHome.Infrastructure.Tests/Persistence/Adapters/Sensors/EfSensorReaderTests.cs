using Microsoft.EntityFrameworkCore;
using SmartHome.Domain.Devices;
using SmartHome.Domain.Sensors;
using SmartHome.Infrastructure.Persistence;
using SmartHome.Infrastructure.Persistence.Adapters.Sensors;
using Testcontainers.PostgreSql;

namespace SmartHome.Infrastructure.Tests.Persistence.Adapters.Sensors;

public class EfSensorReaderTests : IAsyncLifetime
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
    public async Task GetByIdAsync_ReturnsSensor_WhenSensorExists()
    {
        // Arrange
        Device device = new Device("Device Name", "Device Model");
        Sensor expectedSensor = new Sensor(device.Id, SensorCategory.Climate, "Sensor Name", "Sensor Model");

        _dbContext.Devices.Add(device);
        _dbContext.Sensors.Add(expectedSensor);
        await _dbContext.SaveChangesAsync();

        EfSensorReader reader = new(_dbContext);

        // Act
        Sensor? result = await reader.GetByIdAsync(expectedSensor.Id, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(expectedSensor.Id, result.Id);
        Assert.Equal(expectedSensor.DeviceId, result.DeviceId);
        Assert.Equal(expectedSensor.Category, result.Category);
        Assert.Equal(expectedSensor.Name, result.Name);
        Assert.Equal(expectedSensor.Model, result.Model);
        
        Assert.Equal(
            EntityState.Detached,
            _dbContext.Entry(result).State
        );
    }

    [Fact]
    public async Task GetByIdAsync_ReturnsNull_WhenSensorDoesNotExist()
    {
        // Arrange
        EfSensorReader reader = new(_dbContext);

        SensorId sensorId = SensorId.New();

        // Act
        Sensor? result = await reader.GetByIdAsync(
            sensorId,
            CancellationToken.None
        );

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task GetAllAsync_ReturnsAllSensors()
    {
        // Arrange
        Device firstDevice = new("First Device", "First Model");
        Device secondDevice = new("Second Device", "Second Model");

        Sensor firstSensor = new(firstDevice.Id, SensorCategory.Climate, "First Sensor", "First Sensor Model");
        Sensor secondSensor = new(firstDevice.Id, SensorCategory.Climate, "Second Sensor", "Second Sensor Model");
        Sensor thirdSensor = new(secondDevice.Id, SensorCategory.Climate, "Third Sensor", "Third Sensor Model");

        _dbContext.Devices.AddRange(firstDevice, secondDevice);
        _dbContext.Sensors.AddRange(firstSensor, secondSensor, thirdSensor);
        await _dbContext.SaveChangesAsync();

        EfSensorReader reader = new(_dbContext);

        // Act
        IReadOnlyList<Sensor> result = await reader.GetAllAsync(CancellationToken.None);

        // Assert
        Assert.Equal(3, result.Count);

        Assert.Contains(result, sensor => sensor.Id == firstSensor.Id);
        Assert.Contains(result, sensor => sensor.Id == secondSensor.Id);
        Assert.Contains(result, sensor => sensor.Id == thirdSensor.Id);

        Assert.All(
            result,
            sensor => Assert.Equal(
                EntityState.Detached,
                _dbContext.Entry(sensor).State
            )
        );
    }

    [Fact]
    public async Task GetAllAsync_ReturnsEmptyCollection_WhenNoSensorsExist()
    {
        // Arrange
        EfSensorReader reader = new(_dbContext);

        // Act
        IReadOnlyList<Sensor> result = await reader.GetAllAsync(
            CancellationToken.None
        );

        // Assert
        Assert.Empty(result);
    }
}
