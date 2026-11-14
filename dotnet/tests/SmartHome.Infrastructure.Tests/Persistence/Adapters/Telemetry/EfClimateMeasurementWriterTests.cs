using Microsoft.EntityFrameworkCore;
using SmartHome.Domain.Devices;
using SmartHome.Domain.Measurements;
using SmartHome.Domain.Sensors;
using SmartHome.Infrastructure.Persistence;
using SmartHome.Infrastructure.Persistence.Adapters.Telemetry;
using Testcontainers.PostgreSql;

namespace SmartHome.Infrastructure.Tests.Persistence.Adapters.Telemetry;

public class EfClimateMeasurementWriterTests : IAsyncLifetime
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
    public async Task TryAddAsync_ReturnsTrue_WhenMeasurementIsNew()
    {
        // Arrange
        Sensor sensor = await CreateSensorAsync();

        ClimateMeasurement measurement = CreateMeasurement(
            sourceMessageId: new SourceMessageId(1),
            sensorId: sensor.Id
        );

        EfClimateMeasurementWriter writer = new(_dbContext);

        // Act
        bool result = await writer.TryAddAsync(
            measurement,
            CancellationToken.None
        );

        // Assert
        Assert.True(result);

        ClimateMeasurement? persistedMeasurement =
            await _dbContext.ClimateMeasurements
                .AsNoTracking()
                .SingleOrDefaultAsync(
                    item => item.Id == measurement.Id
                );

        Assert.NotNull(persistedMeasurement);
        Assert.Equal(measurement.Id, persistedMeasurement.Id);
    }

    [Fact]
    public async Task TryAddAsync_ReturnsFalse_WhenMeasurementIsDuplicate()
    {
        //Arrange
        Sensor sensor = await CreateSensorAsync();

        ClimateMeasurement measurement = CreateMeasurement(
            sourceMessageId: new SourceMessageId(1),
            sensorId: sensor.Id
        );

        _dbContext.ClimateMeasurements.Add(measurement);
        await _dbContext.SaveChangesAsync();

        ClimateMeasurement duplicateMeasurement = CreateMeasurement(
            sourceMessageId: measurement.SourceMessageId,
            sensorId: sensor.Id
        );

        EfClimateMeasurementWriter writer = new(_dbContext);

        // Act
        bool result = await writer.TryAddAsync(
            duplicateMeasurement,
            CancellationToken.None
        );

        // Assert
        Assert.False(result);

        int measurementCount = await _dbContext.ClimateMeasurements
            .CountAsync(item =>
                item.SensorId == sensor.Id &&
                item.SourceMessageId == measurement.SourceMessageId
            );
        
        Assert.Equal(1, measurementCount);
    }

    [Fact]
    public async Task TryAddAsync_DetachesMeasurement_WhenMeasurementIsDuplicate()
    {
        // Arrange
        Sensor sensor = await CreateSensorAsync();

        ClimateMeasurement measurement = CreateMeasurement(
            sourceMessageId: new SourceMessageId(1),
            sensorId: sensor.Id
        );

        _dbContext.ClimateMeasurements.Add(measurement);
        await _dbContext.SaveChangesAsync();

        ClimateMeasurement duplicateMeasurement = CreateMeasurement(
            sourceMessageId: measurement.SourceMessageId,
            sensorId: sensor.Id
        );

        EfClimateMeasurementWriter writer = new(_dbContext);

        // Act
        await writer.TryAddAsync(
            duplicateMeasurement,
            CancellationToken.None
        );

        // Assert
        Assert.Equal(
            EntityState.Detached,
            _dbContext.Entry(duplicateMeasurement).State
        );
    }

    [Fact]
    public async Task TryAddAsync_ThrowsWhenDatabaseErrorIsNotDuplicate()
    {
        // Arrange
        ClimateMeasurement measurement = CreateMeasurement(
            sourceMessageId: new SourceMessageId(1),
            sensorId: new SensorId(Guid.NewGuid())
        );

        EfClimateMeasurementWriter writer = new(_dbContext);

        // Act
        Func<Task> act = () => writer.TryAddAsync(measurement, CancellationToken.None);

        // Assert
        await Assert.ThrowsAsync<DbUpdateException>(act);
    }

    private async Task<Sensor> CreateSensorAsync()
    {
        Device device = new("Device Name", "Device Model");

        _dbContext.Devices.Add(device);
        await _dbContext.SaveChangesAsync();

        Sensor sensor = new Sensor(device.Id, SensorCategory.Climate, "Sensor Name", "Sensor Model");

        _dbContext.Sensors.Add(sensor);
        await _dbContext.SaveChangesAsync();

        return sensor;
    }

    private static ClimateMeasurement CreateMeasurement(
        SourceMessageId? sourceMessageId = null,
        SensorId? sensorId = null,
        DateTime? measuredAtUtc = null,
        DateTime? receivedAtUtc = null,
        decimal? temperatureCelsius = null,
        decimal? relativeHumidityPercent = null)
    {
        return new ClimateMeasurement(
            sourceMessageId ?? new SourceMessageId(42),
            sensorId ?? SensorId.New(),
            measuredAtUtc ?? new DateTime(2026, 6, 6, 12, 0, 0, DateTimeKind.Utc),
            receivedAtUtc ?? new DateTime(2026, 6, 6, 12, 0, 1, DateTimeKind.Utc),
            temperatureCelsius ?? 25.0m,
            relativeHumidityPercent ?? 50.0m
        );
    }
}
