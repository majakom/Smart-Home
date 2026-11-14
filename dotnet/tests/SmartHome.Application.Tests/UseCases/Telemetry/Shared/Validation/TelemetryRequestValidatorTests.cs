using SmartHome.Application.UseCases.Telemetry.Shared.Results;
using SmartHome.Application.UseCases.Telemetry.Shared.Validation;
using SmartHome.Domain.Devices;
using SmartHome.Domain.Measurements;
using SmartHome.Domain.Sensors;

namespace SmartHome.Application.Tests.UseCases.Telemetry.Shared.Validation;

public class TelemetryRequestValidatorTests
{
    private readonly TelemetryRequestValidator _validator = new();

    [Fact]
    public void Validate_ReturnsNull_WhenRequestIsValid()
    {
        // Arrange
        SourceMessageId sourceMessageId = new SourceMessageId(1);
        DeviceId deviceId = DeviceId.New();
        SensorId sensorId = SensorId.New();
        DateTime measuredAtUtc = ValidTimestamp();

        // Act
        TelemetryRejectionReason? result = _validator.Validate(
            sourceMessageId,
            deviceId,
            sensorId,
            measuredAtUtc
        );

        // Assert
        Assert.Null(result);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Validate_ReturnsInvalidSourceMessageId_WhenSourceMessageIdIsNotPositive(long value)
    {
        // Arrange
        SourceMessageId sourceMessageId = new SourceMessageId(value);

        // Act
        TelemetryRejectionReason? result = _validator.Validate(
            sourceMessageId,
            DeviceId.New(),
            SensorId.New(),
            ValidTimestamp()
        );

        // Assert
        Assert.Equal(TelemetryRejectionReason.InvalidSourceMessageId, result);
    }

    [Fact]
    public void Validate_ReturnsInvalidDeviceId_WhenDeviceIdIsEmpty()
    {
        // Arrange
        DeviceId deviceId = default;

        // Act
        TelemetryRejectionReason? result = _validator.Validate(
            new SourceMessageId(1),
            deviceId,
            SensorId.New(),
            ValidTimestamp()
        );

        // Assert
        Assert.Equal(TelemetryRejectionReason.InvalidDeviceId, result);
    }

    [Fact]
    public void Validate_ReturnsInvalidSensorId_WhenSensorIdIsEmpty()
    {
        // Arrange
        SensorId sensorId = default;

        // Act
        TelemetryRejectionReason? result = _validator.Validate(
            new SourceMessageId(1),
            DeviceId.New(),
            sensorId,
            ValidTimestamp()
        );

        // Assert
        Assert.Equal(TelemetryRejectionReason.InvalidSensorId, result);
    }

    [Theory]
    [InlineData(DateTimeKind.Local)]
    [InlineData(DateTimeKind.Unspecified)]
    public void Validate_ReturnsInvalidMeasuredAtUtc_WhenTimestampIsNotUtc(
        DateTimeKind kind)
    {
        // Arrange
        DateTime measuredAtUtc = new DateTime(2026, 6, 6, 12, 0, 0, kind);

        // Act
        TelemetryRejectionReason? result = _validator.Validate(
            new SourceMessageId(1),
            DeviceId.New(),
            SensorId.New(),
            measuredAtUtc
        );

        // Assert
        Assert.Equal(TelemetryRejectionReason.InvalidMeasuredAtUtc, result);
    }

    [Fact]
    public void Validate_ReturnsInvalidMeasuredAtUtc_WhenTimestampIsDefault()
    {
        // Arrange
        DateTime measuredAtUtc = default;

        // Act
        TelemetryRejectionReason? result = _validator.Validate(
            new SourceMessageId(1),
            DeviceId.New(),
            SensorId.New(),
            measuredAtUtc
        );

        // Assert
        Assert.Equal(TelemetryRejectionReason.InvalidMeasuredAtUtc, result);
    }

    private static DateTime ValidTimestamp()
    {
        return new(2026, 6, 6, 12, 0, 0, DateTimeKind.Utc);
    }
}
