using SmartHome.Application.UseCases.Telemetry.RecordClimateMeasurement;
using SmartHome.Application.UseCases.Telemetry.Shared.Results;
using SmartHome.Application.UseCases.Telemetry.Shared.Validation;
using SmartHome.Domain.Devices;
using SmartHome.Domain.Measurements;
using SmartHome.Domain.Sensors;

namespace SmartHome.Application.Tests.UseCases.Telemetry.RecordClimateMeasurement;

public class RecordClimateMeasurementRequestValidatorTests
{
    private readonly TelemetryRequestValidator _telemetryRequestValidator = new();
    private readonly RecordClimateMeasurementRequestValidator _validator;

    public RecordClimateMeasurementRequestValidatorTests()
    {
        _validator = new RecordClimateMeasurementRequestValidator(_telemetryRequestValidator);
    }

    [Fact]
    public void Validate_ReturnsNull_WhenRequestIsValid()
    {
        // Arrange
        RecordClimateMeasurementRequest request = CreateRequest();

        // Act
        TelemetryRejectionReason? result = _validator.Validate(request);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public void Validate_ThrowsArgumentNullException_WhenRequestIsNull()
    {
        // Arrange
        RecordClimateMeasurementRequest? request = null;

        // Act
        Action act = () => _validator.Validate(request!);

        // Assert
        ArgumentNullException exception = Assert.Throws<ArgumentNullException>(act);
        Assert.Equal("request", exception.ParamName);
    }

    [Fact]
    public void Validate_ReturnsInvalidSourceMessageId_WhenSourceMessageIdIsNotPositive()
    {
        // Arrange
        RecordClimateMeasurementRequest request = CreateRequest(
            sourceMessageId: new SourceMessageId(0)
        );

        // Act
        TelemetryRejectionReason? result = _validator.Validate(request);

        // Assert
        Assert.Equal(TelemetryRejectionReason.InvalidSourceMessageId, result);
    }

    [Fact]
    public void Validate_ReturnsInvalidDeviceId_WhenDeviceIdIsEmpty()
    {
        // Arrange
        RecordClimateMeasurementRequest request = CreateRequest() with
        {
            DeviceId = default
        };

        // Act
        TelemetryRejectionReason? result = _validator.Validate(request);

        // Assert
        Assert.Equal(TelemetryRejectionReason.InvalidDeviceId, result);
    }

    [Fact]
    public void Validate_ReturnsInvalidSensorId_WhenSensorIdIsEmpty()
    {
        // Arrange
        RecordClimateMeasurementRequest request = CreateRequest() with
        {
            SensorId = default
        };

        // Act
        TelemetryRejectionReason? result = _validator.Validate(request);

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
        RecordClimateMeasurementRequest request = CreateRequest(
            measuredAtUtc: new(2026, 6, 6, 12, 0, 0, kind)
        );

        // Act
        TelemetryRejectionReason? result = _validator.Validate(request);

        // Assert
        Assert.Equal(TelemetryRejectionReason.InvalidMeasuredAtUtc, result);
    }

    [Fact]
    public void Validate_ReturnsInvalidMeasuredAtUtc_WhenTimestampIsDefault()
    {
        // Arrange
        RecordClimateMeasurementRequest request = CreateRequest() with
        {
            MeasuredAtUtc = default
        };

        // Act
        TelemetryRejectionReason? result = _validator.Validate(request);

        // Assert
        Assert.Equal(TelemetryRejectionReason.InvalidMeasuredAtUtc, result);
    }

    [Theory]
    [InlineData(-0.01)]
    [InlineData(100.01)]
    public void Validate_ReturnsInvalidMeasurementData_WhenHumidityIsOutsideValidRange(double humidity)
    {
        // Arrange
        RecordClimateMeasurementRequest request = CreateRequest(
            relativeHumidityPercent: (decimal)humidity
        );

        // Act
        TelemetryRejectionReason? result = _validator.Validate(request);

        // Assert
        Assert.Equal(TelemetryRejectionReason.InvalidMeasurementData, result);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(100)]
    public void Validate_ReturnsNull_WhenHumidityIsAtValidBoundary(double humidity)
    {
        // Arrange
        RecordClimateMeasurementRequest request = CreateRequest(
            relativeHumidityPercent: (decimal)humidity
        );

        // Act
        TelemetryRejectionReason? result = _validator.Validate(request);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public void Validate_ReturnsNull_WhenHumidityIsNull()
    {
        // Arrange
        RecordClimateMeasurementRequest request = CreateRequest(
            relativeHumidityPercent: null
        );

        // Act
        TelemetryRejectionReason? result = _validator.Validate(request);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public void Validate_ReturnsNull_WhenTemperatureIsNull()
    {
        // Arrange
        RecordClimateMeasurementRequest request = CreateRequest(
            temperatureCelsius: null
        );

        // Act
        TelemetryRejectionReason? result = _validator.Validate(request);

        // Assert
        Assert.Null(result);
    }

    private static RecordClimateMeasurementRequest CreateRequest(
        SourceMessageId? sourceMessageId = null,
        SensorId? sensorId = null,
        DeviceId? deviceId = null,
        DateTime? measuredAtUtc = null,
        decimal? temperatureCelsius = 21.5m,
        decimal? relativeHumidityPercent = 50m)
    {
        return new RecordClimateMeasurementRequest(
            sourceMessageId ?? new SourceMessageId(1),
            sensorId ?? SensorId.New(),
            deviceId ?? DeviceId.New(),
            measuredAtUtc ?? new DateTime(2026, 6, 6, 12, 0, 0, DateTimeKind.Utc),
            temperatureCelsius,
            relativeHumidityPercent
        );
    }
}
