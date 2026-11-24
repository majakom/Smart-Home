using NSubstitute;
using SmartHome.Application.Abstractions.Sensors;
using SmartHome.Application.UseCases.Telemetry.Shared.Results;
using SmartHome.Application.UseCases.Telemetry.Shared.Validation;
using SmartHome.Domain.Devices;
using SmartHome.Domain.Sensors;

namespace SmartHome.Application.Tests.UseCases.Telemetry.Shared.Validation;

public class TelemetryQuerySensorValidatorTests
{
    private readonly ISensorReader _sensorReader;
    private readonly TelemetryQuerySensorValidator _validator;

    public TelemetryQuerySensorValidatorTests()
    {
        _sensorReader = Substitute.For<ISensorReader>();

        _validator = new TelemetryQuerySensorValidator(
            _sensorReader
        );
    }

    [Fact]
    public async Task ValidateAsync_ReturnsInvalidSensorId_WhenSensorIdIsEmpty()
    {
        // Arrange
        SensorId sensorId = default;
        CancellationToken cancellationToken = CancellationToken.None;

        // Act
        TelemetryQueryFailureReason? result = await _validator.ValidateAsync(
            sensorId,
            SensorCategory.Climate,
            cancellationToken
        );

        // Assert
        Assert.Equal(TelemetryQueryFailureReason.InvalidSensorId, result);

        await _sensorReader.DidNotReceive().GetByIdAsync(
            Arg.Any<SensorId>(),
            Arg.Any<CancellationToken>()
        );
    }

    [Fact]
    public async Task ValidateAsync_ReturnsSensorNotFound_WhenSensorDoesNotExist()
    {
        // Arrange
        SensorId sensorId = SensorId.New();
        CancellationToken cancellationToken = CancellationToken.None;

        _sensorReader.GetByIdAsync(sensorId, cancellationToken)
            .Returns(Task.FromResult<Sensor?>(null));

        // Act
        TelemetryQueryFailureReason? result = await _validator.ValidateAsync(
            sensorId,
            SensorCategory.Climate,
            cancellationToken
        );

        // Assert
        Assert.Equal(TelemetryQueryFailureReason.SensorNotFound, result);

        await _sensorReader.Received(1).GetByIdAsync(
            sensorId,
            cancellationToken
        );
    }

    [Fact]
    public async Task ValidateAsync_ReturnsInvalidSensorCategory_WhenCategoryDoesNotMatch()
    {
        // Arrange
        Device device = CreateDevice();
        Sensor sensor = CreateSensor(device.Id);
        SensorCategory invalidCategory = (SensorCategory)999;
        CancellationToken cancellationToken = CancellationToken.None;

        _sensorReader.GetByIdAsync(sensor.Id, cancellationToken)
            .Returns(Task.FromResult<Sensor?>(sensor));

        // Act
        TelemetryQueryFailureReason? result = await _validator.ValidateAsync(
            sensor.Id,
            invalidCategory,
            cancellationToken
        );

        // Assert
        Assert.Equal(TelemetryQueryFailureReason.InvalidSensorCategory, result);
    }

    [Fact]
    public async Task ValidateAsync_ReturnsNull_WhenValidationSucceeds()
    {
        // Arrange
        Device device = CreateDevice();
        Sensor sensor = CreateSensor(device.Id);
        CancellationToken cancellationToken = CancellationToken.None;

        _sensorReader.GetByIdAsync(sensor.Id, cancellationToken)
            .Returns(Task.FromResult<Sensor?>(sensor));

        // Act
        TelemetryQueryFailureReason? result = await _validator.ValidateAsync(
            sensor.Id,
            SensorCategory.Climate,
            cancellationToken
        );

        // Assert
        Assert.Null(result);

        await _sensorReader.Received(1).GetByIdAsync(
            sensor.Id,
            cancellationToken
        );
    }

    private static Device CreateDevice()
    {
        return new Device(
            "Device Name",
            "Device Model"
        );
    }
    
    private static Sensor CreateSensor(DeviceId deviceId)
    {
        return new Sensor(
            deviceId,
            SensorCategory.Climate,
            "Sensor Name",
            "Sensor Model"
        );
    }
}
