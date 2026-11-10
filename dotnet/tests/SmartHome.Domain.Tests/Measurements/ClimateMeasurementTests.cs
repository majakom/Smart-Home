using SmartHome.Domain.Measurements;
using SmartHome.Domain.Sensors;

namespace SmartHome.Domain.Tests.Measurements;

public class ClimateMeasurementTests
{
    [Fact]
    public void Constructor_CreatesMeasurement_WhenArgumentsAreValid()
    {
        // Arrange
        SourceMessageId sourceMessageId = new(42);
        SensorId sensorId = new(Guid.NewGuid());
        DateTime measuredAtUtc = new(2026, 6, 6, 12, 0, 0, DateTimeKind.Utc);
        DateTime receivedAtUtc = new(2026, 6, 6, 12, 0, 1, DateTimeKind.Utc);
        decimal temperatureCelsius = 21.5m;
        decimal relativeHumidityPercent = 45.2m;

        // Act
        ClimateMeasurement measurement = new(
            sourceMessageId,
            sensorId,
            measuredAtUtc,
            receivedAtUtc,
            temperatureCelsius,
            relativeHumidityPercent
        );

        // Assert
        Assert.False(measurement.Id.IsEmpty);
        Assert.Equal(sourceMessageId, measurement.SourceMessageId);
        Assert.Equal(sensorId, measurement.SensorId);
        Assert.Equal(measuredAtUtc, measurement.MeasuredAtUtc);
        Assert.Equal(receivedAtUtc, measurement.ReceivedAtUtc);
        Assert.Equal(temperatureCelsius, measurement.TemperatureCelsius);
        Assert.Equal(relativeHumidityPercent, measurement.RelativeHumidityPercent);
    }

    [Fact]
    public void Constructor_SetsMeasurementsToNull_WhenOptionalArgumentsAreOmitted()
    {
        // Arrange
        SourceMessageId sourceMessageId = new(42);
        SensorId sensorId = SensorId.New();
        DateTime measuredAtUtc = new(2026, 6, 6, 12, 0, 0, DateTimeKind.Utc);
        DateTime receivedAtUtc = new(2026, 6, 6, 12, 0, 1, DateTimeKind.Utc);

        // Act
        ClimateMeasurement measurement = new(
            sourceMessageId,
            sensorId,
            measuredAtUtc,
            receivedAtUtc
        );

        // Assert
        Assert.Null(measurement.TemperatureCelsius);
        Assert.Null(measurement.RelativeHumidityPercent);
    }

    [Fact]
    public void Constructor_ThrowsArgumentOutOfRangeException_WhenSourceMessageIdIsZero()
    {
        // Arrange
        SourceMessageId sourceMessageId = new(0);

        // Act
        Action act = () => CreateMeasurement(sourceMessageId: sourceMessageId);

        // Assert
        ArgumentOutOfRangeException exception = Assert.Throws<ArgumentOutOfRangeException>(act);
        Assert.Equal("sourceMessageId", exception.ParamName);
    }

    [Fact]
    public void Constructor_ThrowsArgumentOutOfRangeException_WhenSourceMessageIdIsNegative()
    {
        // Arrange
        SourceMessageId sourceMessageId = new(-1);

        // Act
        Action act = () => CreateMeasurement(sourceMessageId: sourceMessageId);

        // Assert
        ArgumentOutOfRangeException exception = Assert.Throws<ArgumentOutOfRangeException>(act);
        Assert.Equal("sourceMessageId", exception.ParamName);
    }

    [Fact]
    public void Constructor_ThrowsArgumentException_WhenSensorIdIsEmpty()
    {
        // Arrange
        SensorId sensorId = default;

        // Act
        Action act = () => CreateMeasurement(sensorId: sensorId);

        // Assert
        ArgumentException exception = Assert.Throws<ArgumentException>(act);
        Assert.Equal("sensorId", exception.ParamName);
    }

    [Theory]
    [InlineData(DateTimeKind.Local)]
    [InlineData(DateTimeKind.Unspecified)]
    public void Constructor_ThrowsArgumentException_WhenMeasuredAtUtcIsNotUtc(DateTimeKind kind)
    {
        // Arrange
        DateTime measuredAt = new(2026, 6, 6, 12, 0, 0, kind);

        // Act
        Action act = () => CreateMeasurement(measuredAtUtc: measuredAt);

        // Assert
        ArgumentException exception = Assert.Throws<ArgumentException>(act);
        Assert.Equal("measuredAtUtc", exception.ParamName);
    }

    [Theory]
    [InlineData(DateTimeKind.Local)]
    [InlineData(DateTimeKind.Unspecified)]
    public void Constructor_ThrowsArgumentException_WhenReceivedAtUtcIsNotUtc(DateTimeKind kind)
    {
        // Arrange
        DateTime receivedAt = new(2026, 6, 6, 12, 0, 0, kind);

        // Act
        Action act = () => CreateMeasurement(receivedAtUtc: receivedAt);

        // Assert
        ArgumentException exception = Assert.Throws<ArgumentException>(act);
        Assert.Equal("receivedAtUtc", exception.ParamName);
    }

    [Theory]
    [InlineData(-0.01)]
    [InlineData(100.01)]
    public void Constructor_ThrowsArgumentOutOfRangeException_WhenHumidityIsOutsideValidRange(
        double humidity)
    {
        // Arrange
        decimal relativeHumidityPercent = (decimal)humidity;

        // Act
        Action act = () => CreateMeasurement(
            relativeHumidityPercent: relativeHumidityPercent
        );

        // Assert
        ArgumentOutOfRangeException exception = Assert.Throws<ArgumentOutOfRangeException>(act);
        Assert.Equal("relativeHumidityPercent", exception.ParamName);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(100)]
    public void Constructor_AcceptsHumidity_WhenValueIsAtValidBoundary(double humidity)
    {
        // Arrange
        decimal relativeHumidityPercent = (decimal)humidity;

        // Act
        ClimateMeasurement measurement = CreateMeasurement(
            relativeHumidityPercent: relativeHumidityPercent
        );

        // Assert
        Assert.Equal(relativeHumidityPercent, measurement.RelativeHumidityPercent);
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
            temperatureCelsius,
            relativeHumidityPercent
        );
    }
}