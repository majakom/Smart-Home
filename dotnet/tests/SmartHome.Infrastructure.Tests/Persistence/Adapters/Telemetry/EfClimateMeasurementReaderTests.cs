
using Microsoft.EntityFrameworkCore;
using SmartHome.Domain.Devices;
using SmartHome.Domain.Measurements;
using SmartHome.Domain.Sensors;
using SmartHome.Infrastructure.Persistence;
using SmartHome.Infrastructure.Persistence.Adapters.Telemetry;
using Testcontainers.PostgreSql;

namespace SmartHome.Infrastructure.Tests.Persistence.Adapters.Telemetry;

public class EfClimateMeasurementReaderTests : IAsyncLifetime
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
    public async Task GetBySensorIdAsync_ReturnsMeasurements_WhenMeasurementsExist()
    {
        // Arrange
        Device device = new("Device Name", "Device Model");

        Sensor sensor = new(device.Id, SensorCategory.Climate, "Sensor Name", "Sensor Model");

        DateTime fromUtc = new(2026, 6, 6, 10, 0, 0, DateTimeKind.Utc);
        DateTime toUtc = new(2026, 6, 6, 12, 0, 0, DateTimeKind.Utc);

        ClimateMeasurement firstMeasurement = new(
            new SourceMessageId(1),
            sensor.Id,
            fromUtc.AddMinutes(30),
            fromUtc.AddMinutes(31),
            21.5m,
            45m
        );
        ClimateMeasurement secondMeasurement = new(
            new SourceMessageId(2),
            sensor.Id,
            fromUtc.AddMinutes(60),
            fromUtc.AddMinutes(61),
            22m,
            46m
        );

        _dbContext.Devices.Add(device);
        _dbContext.Sensors.Add(sensor);
        _dbContext.ClimateMeasurements.AddRange(firstMeasurement, secondMeasurement);
        await _dbContext.SaveChangesAsync();

        EfClimateMeasurementReader reader = new(_dbContext);

        // Act
        IReadOnlyList<ClimateMeasurement> result =
            await reader.GetBySensorIdAsync(
                sensor.Id,
                fromUtc,
                toUtc,
                100,
                CancellationToken.None
            );

        // Assert
        Assert.Equal(2, result.Count);

        Assert.Equal(firstMeasurement.Id, result[0].Id);
        Assert.Equal(secondMeasurement.Id, result[1].Id);

        Assert.All(
            result,
            measurement => Assert.Equal(
                EntityState.Detached,
                _dbContext.Entry(measurement).State
            )
        );
    }

    [Fact]
    public async Task GetBySensorIdAsync_ReturnsNoMoreThanLimit()
    {
        // Arrange
        Device device = new("Device Name", "Device Model");
        Sensor sensor = new(device.Id, SensorCategory.Climate, "Sensor Name", "Sensor Model");

        DateTime fromUtc = new(2026, 6, 6, 10, 0, 0, DateTimeKind.Utc);
        DateTime toUtc = fromUtc.AddHours(1);

        ClimateMeasurement[] measurements =
        [
            new(new SourceMessageId(1), sensor.Id, fromUtc.AddMinutes(10), fromUtc.AddMinutes(11), 20m, 40m),
            new(new SourceMessageId(2), sensor.Id, fromUtc.AddMinutes(20), fromUtc.AddMinutes(21), 21m, 41m),
            new(new SourceMessageId(3), sensor.Id, fromUtc.AddMinutes(30), fromUtc.AddMinutes(31), 22m, 42m)
        ];

        _dbContext.Devices.Add(device);
        _dbContext.Sensors.Add(sensor);
        _dbContext.ClimateMeasurements.AddRange(measurements);
        await _dbContext.SaveChangesAsync();

        EfClimateMeasurementReader reader = new(_dbContext);
        const int limit = 2;

        // Act
        IReadOnlyList<ClimateMeasurement> result =
            await reader.GetBySensorIdAsync(
                sensor.Id,
                fromUtc,
                toUtc,
                limit,
                CancellationToken.None
            );

        // Assert
        Assert.Equal(limit, result.Count);
    }

    [Fact]
    public async Task GetBySensorIdAsync_ReturnsEarliestMeasurementsUpToLimit()
    {
        // Arrange
        Device device = new("Device Name", "Device Model");
        Sensor sensor = new(device.Id, SensorCategory.Climate, "Sensor Name", "Sensor Model");

        DateTime fromUtc = new(2026, 6, 6, 10, 0, 0, DateTimeKind.Utc);
        DateTime toUtc = fromUtc.AddHours(1);

        ClimateMeasurement latestMeasurement = new(
            new SourceMessageId(1), sensor.Id, fromUtc.AddMinutes(30), fromUtc.AddMinutes(31), 22m, 42m);
        ClimateMeasurement earliestMeasurement = new(
            new SourceMessageId(2), sensor.Id, fromUtc.AddMinutes(10), fromUtc.AddMinutes(11), 20m, 40m);
        ClimateMeasurement middleMeasurement = new(
            new SourceMessageId(3), sensor.Id, fromUtc.AddMinutes(20), fromUtc.AddMinutes(21), 21m, 41m);

        _dbContext.Devices.Add(device);
        _dbContext.Sensors.Add(sensor);
        _dbContext.ClimateMeasurements.AddRange(
            latestMeasurement,
            earliestMeasurement,
            middleMeasurement
        );
        await _dbContext.SaveChangesAsync();

        EfClimateMeasurementReader reader = new(_dbContext);
        const int limit = 2;

        // Act
        IReadOnlyList<ClimateMeasurement> result =
            await reader.GetBySensorIdAsync(
                sensor.Id,
                fromUtc,
                toUtc,
                limit,
                CancellationToken.None
            );

        // Assert
        Assert.Equal(limit, result.Count);
        Assert.Equal(earliestMeasurement.Id, result[0].Id);
        Assert.Equal(middleMeasurement.Id, result[1].Id);
    }

    [Fact]
    public async Task GetBySensorIdAsync_OrdersByMeasuredAtUtcThenId()
    {
        // Arrange
        Device device = new("Device Name", "Device Model");
        Sensor sensor = new(device.Id, SensorCategory.Climate, "Sensor Name", "Sensor Model");

        DateTime fromUtc = new(2026, 6, 6, 10, 0, 0, DateTimeKind.Utc);
        DateTime toUtc = fromUtc.AddHours(1);
        DateTime sameMeasuredAtUtc = fromUtc.AddMinutes(10);

        ClimateMeasurement laterMeasurement = new(
            new SourceMessageId(1), sensor.Id, fromUtc.AddMinutes(20), fromUtc.AddMinutes(21), 22m, 42m);
        ClimateMeasurement firstAtSameTime = new(
            new SourceMessageId(2), sensor.Id, sameMeasuredAtUtc, fromUtc.AddMinutes(11), 20m, 40m);
        ClimateMeasurement secondAtSameTime = new(
            new SourceMessageId(3), sensor.Id, sameMeasuredAtUtc, fromUtc.AddMinutes(12), 21m, 41m);

        _dbContext.Devices.Add(device);
        _dbContext.Sensors.Add(sensor);
        _dbContext.ClimateMeasurements.AddRange(
            laterMeasurement,
            firstAtSameTime,
            secondAtSameTime
        );
        await _dbContext.SaveChangesAsync();

        EfClimateMeasurementReader reader = new(_dbContext);

        // Act
        IReadOnlyList<ClimateMeasurement> result =
            await reader.GetBySensorIdAsync(
                sensor.Id,
                fromUtc,
                toUtc,
                100,
                CancellationToken.None
            );

            // Assert
        Assert.Equal(
            [firstAtSameTime.Id, secondAtSameTime.Id, laterMeasurement.Id],
            result.Select(measurement => measurement.Id));
    }

    [Fact]
    public async Task GetBySensorIdAsync_ExcludesMeasurementsFromOtherSensors()
    {
        // Arrange
        Device device = new("Device Name", "Device Model");

        Sensor firstSensor = new(device.Id, SensorCategory.Climate, "First Sensor", "First Model");
        Sensor secondSensor = new(device.Id, SensorCategory.Climate, "Second Sensor", "Second Model");

        DateTime fromUtc = new(2026, 11, 18, 10, 0, 0, DateTimeKind.Utc);
        DateTime toUtc = new(2026, 11, 18, 12, 0, 0, DateTimeKind.Utc);

        ClimateMeasurement expectedMeasurement = new(
            new SourceMessageId(1),
            firstSensor.Id,
            fromUtc.AddMinutes(30),
            fromUtc.AddMinutes(31),
            21.5m,
            45m
        );

        ClimateMeasurement otherMeasurement = new(
            new SourceMessageId(2),
            secondSensor.Id,
            fromUtc.AddMinutes(30),
            fromUtc.AddMinutes(31),
            22m,
            46m
        );

        _dbContext.Devices.Add(device);
        _dbContext.Sensors.AddRange(firstSensor, secondSensor);
        _dbContext.ClimateMeasurements.AddRange(expectedMeasurement, otherMeasurement);

        await _dbContext.SaveChangesAsync();

        EfClimateMeasurementReader reader = new(_dbContext);

        // Act
        IReadOnlyList<ClimateMeasurement> result =
            await reader.GetBySensorIdAsync(
                firstSensor.Id,
                fromUtc,
                toUtc,
                100,
                CancellationToken.None
            );

        // Assert
        Assert.Single(result);
        Assert.Equal(expectedMeasurement.Id, result[0].Id);
    }

    [Fact]
    public async Task GetBySensorIdAsync_ReturnsEmptyCollection_WhenNoMeasurementsExist()
    {
        // Arrange
        EfClimateMeasurementReader reader = new(_dbContext);

        SensorId sensorId = SensorId.New();

        DateTime fromUtc = new(2026, 11, 18, 10, 0, 0, DateTimeKind.Utc);
        DateTime toUtc = new(2026, 11, 18, 12, 0, 0, DateTimeKind.Utc);

        // Act
        IReadOnlyList<ClimateMeasurement> result =
            await reader.GetBySensorIdAsync(
                sensorId,
                fromUtc,
                toUtc,
                100,
                CancellationToken.None
            );

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public async Task GetBySensorIdAsync_UsesInclusiveTimeRange()
    {
        // Arrange
        Device device = new("Device Name", "Device Model");

        Sensor sensor = new(device.Id, SensorCategory.Climate, "Sensor Name", "Sensor Model");

        DateTime fromUtc = new(2026, 12, 12, 10, 0, 0, DateTimeKind.Utc);
        DateTime toUtc = new(2026, 12, 12, 12, 0, 0, DateTimeKind.Utc);

        ClimateMeasurement beforeStart = new(
            new SourceMessageId(1),
            sensor.Id,
            fromUtc.AddSeconds(-1),
            fromUtc,
            20m,
            40m
        );

        ClimateMeasurement atStart = new(
            new SourceMessageId(2),
            sensor.Id,
            fromUtc,
            fromUtc.AddSeconds(1),
            21m,
            41m
        );

        ClimateMeasurement atEnd = new(
            new SourceMessageId(3),
            sensor.Id,
            toUtc,
            toUtc.AddSeconds(1),
            22m,
            42m
        );

        ClimateMeasurement afterEnd = new(
            new SourceMessageId(4),
            sensor.Id,
            toUtc.AddSeconds(1),
            toUtc.AddSeconds(2),
            23m,
            43m
        );

        _dbContext.Devices.Add(device);
        _dbContext.Sensors.Add(sensor);

        _dbContext.ClimateMeasurements.AddRange(
            beforeStart,
            atStart,
            atEnd,
            afterEnd
        );

        await _dbContext.SaveChangesAsync();

        EfClimateMeasurementReader reader = new(_dbContext);

        // Act
        IReadOnlyList<ClimateMeasurement> result =
            await reader.GetBySensorIdAsync(
                sensor.Id,
                fromUtc,
                toUtc,
                100,
                CancellationToken.None
            );

        // Assert
        Assert.Equal(2, result.Count);

        Assert.Equal(atStart.Id, result[0].Id);
        Assert.Equal(atEnd.Id, result[1].Id);

        Assert.DoesNotContain(
            result,
            measurement => measurement.Id == beforeStart.Id
        );

        Assert.DoesNotContain(
            result,
            measurement => measurement.Id == afterEnd.Id
        );
    }
}
