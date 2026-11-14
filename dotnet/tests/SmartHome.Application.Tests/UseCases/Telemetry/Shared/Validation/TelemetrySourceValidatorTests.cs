using NSubstitute;
using SmartHome.Application.Abstractions.Devices;
using SmartHome.Application.Abstractions.Sensors;
using SmartHome.Application.UseCases.Telemetry.Shared.Results;
using SmartHome.Application.UseCases.Telemetry.Shared.Validation;
using SmartHome.Domain.Devices;
using SmartHome.Domain.Sensors;

namespace SmartHome.Application.Tests.UseCases.Telemetry.Shared.Validation;

public class TelemetrySourceValidatorTests
{
    private readonly ISensorReader _sensorReader;
    private readonly IDeviceReader _deviceReader;
    private readonly TelemetrySourceValidator _validator;

    public TelemetrySourceValidatorTests()
    {
        _sensorReader = Substitute.For<ISensorReader>();
        _deviceReader = Substitute.For<IDeviceReader>();

        _validator = new TelemetrySourceValidator(
            _sensorReader,
            _deviceReader
        );
    }

    [Fact]
    public async Task ValidateAsync_ReturnsSensorNotFound_WhenSensorDoesNotExist()
    {
        // Arrange
        SensorId sensorId = SensorId.New();
        DeviceId deviceId = DeviceId.New();
        CancellationToken cancellationToken = CancellationToken.None;

        _sensorReader.GetByIdAsync(sensorId, cancellationToken)
            .Returns(Task.FromResult<Sensor?>(null));

        // Act
        TelemetrySourceValidationResult result = await _validator.ValidateAsync(
            sensorId,
            deviceId,
            SensorCategory.Climate,
            cancellationToken
        );

        // Assert
        Assert.Equal(TelemetryRejectionReason.SensorNotFound, result.RejectionReason);
        Assert.Null(result.Sensor);
        await _deviceReader.DidNotReceive().GetByIdAsync(
            Arg.Any<DeviceId>(),
            Arg.Any<CancellationToken>()
        );
    }

    [Fact]
    public async Task ValidateAsync_ReturnsSensorDoesNotBelongToDevice_WhenSensorBelongsToAnotherDevice()
    {
        // Arrange
        Device device = CreateDevice();
        Sensor sensor = CreateSensor(DeviceId.New());
        CancellationToken cancellationToken = CancellationToken.None;

        _sensorReader.GetByIdAsync(sensor.Id, cancellationToken)
            .Returns(Task.FromResult<Sensor?>(sensor));

        // Act
        TelemetrySourceValidationResult result = await _validator.ValidateAsync(
            sensor.Id,
            device.Id,
            SensorCategory.Climate,
            cancellationToken);

        // Assert
        Assert.Equal(TelemetryRejectionReason.SensorDoesNotBelongToDevice, result.RejectionReason);
        Assert.Null(result.Sensor);

        await _deviceReader.DidNotReceive().GetByIdAsync(
            Arg.Any<DeviceId>(),
            Arg.Any<CancellationToken>()
        );
    }

    [Fact]
    public async Task ValidateAsync_ReturnsDeviceNotFound_WhenDeviceDoesNotExist()
    {
        // Arrange
        Device device = CreateDevice();
        Sensor sensor = CreateSensor(device.Id);
        CancellationToken cancellationToken = CancellationToken.None;

        _sensorReader.GetByIdAsync(sensor.Id, cancellationToken)
            .Returns(Task.FromResult<Sensor?>(sensor));

        _deviceReader.GetByIdAsync(device.Id, cancellationToken)
            .Returns(Task.FromResult<Device?>(null));

        // Act
        TelemetrySourceValidationResult result = await _validator.ValidateAsync(
            sensor.Id,
            device.Id,
            SensorCategory.Climate,
            cancellationToken);

        // Assert
        Assert.Equal(TelemetryRejectionReason.DeviceNotFound, result.RejectionReason);
        Assert.Null(result.Sensor);
    }

    [Fact]
    public async Task ValidateAsync_ReturnsDeviceDisabled_WhenDeviceIsDisabled()
    {
        // Arrange
        Device device = CreateDevice();
        device.Disable();
        Sensor sensor = CreateSensor(device.Id);
        CancellationToken cancellationToken = CancellationToken.None;

        _sensorReader.GetByIdAsync(sensor.Id, cancellationToken)
            .Returns(Task.FromResult<Sensor?>(sensor));

        _deviceReader.GetByIdAsync(device.Id, cancellationToken)
            .Returns(Task.FromResult<Device?>(device));

        // Act
        TelemetrySourceValidationResult result = await _validator.ValidateAsync(
            sensor.Id,
            device.Id,
            SensorCategory.Climate,
            cancellationToken
        );

        // Assert
        Assert.Equal(TelemetryRejectionReason.DeviceDisabled, result.RejectionReason);
        Assert.Null(result.Sensor);
    }

    [Fact]
    public async Task ValidateAsync_ReturnsSensorDisabled_WhenSensorIsDisabled()
    {
        // Arrange
        Device device = CreateDevice();
        Sensor sensor = CreateSensor(device.Id);
        sensor.Disable();
        CancellationToken cancellationToken = CancellationToken.None;

        _sensorReader.GetByIdAsync(sensor.Id, cancellationToken)
            .Returns(Task.FromResult<Sensor?>(sensor));

        _deviceReader.GetByIdAsync(device.Id, cancellationToken)
            .Returns(Task.FromResult<Device?>(device));

        // Act
        TelemetrySourceValidationResult result = await _validator.ValidateAsync(
            sensor.Id,
            device.Id,
            SensorCategory.Climate,
            cancellationToken
        );

        // Assert
        Assert.Equal(TelemetryRejectionReason.SensorDisabled, result.RejectionReason);
        Assert.Null(result.Sensor);
    }

    [Fact]
    public async Task ValidateAsync_ReturnsSensorCategoryMismatch_WhenCategoryDoesNotMatch()
    {
        // Arrange
        Device device = CreateDevice();
        Sensor sensor = CreateSensor(device.Id);
        SensorCategory expectedCategory = (SensorCategory)999;
        CancellationToken cancellationToken = CancellationToken.None;

        _sensorReader.GetByIdAsync(sensor.Id, cancellationToken)
            .Returns(Task.FromResult<Sensor?>(sensor));

        _deviceReader.GetByIdAsync(device.Id, cancellationToken)
            .Returns(Task.FromResult<Device?>(device));

        // Act
        TelemetrySourceValidationResult result = await _validator.ValidateAsync(
            sensor.Id,
            device.Id,
            expectedCategory,
            cancellationToken
        );

        // Assert
        Assert.Equal(TelemetryRejectionReason.SensorCategoryMismatch, result.RejectionReason);
        Assert.Null(result.Sensor);
    }

    [Fact]
    public async Task ValidateAsync_ReturnsSensor_WhenValidationSucceeds()
    {
        // Arrange
        Device device = CreateDevice();
        Sensor sensor = CreateSensor(device.Id);
        CancellationToken cancellationToken = CancellationToken.None;

        _sensorReader.GetByIdAsync(sensor.Id, cancellationToken)
            .Returns(Task.FromResult<Sensor?>(sensor));

        _deviceReader.GetByIdAsync(device.Id, cancellationToken)
            .Returns(Task.FromResult<Device?>(device));

        // Act
        TelemetrySourceValidationResult result = await _validator.ValidateAsync(
            sensor.Id,
            device.Id,
            SensorCategory.Climate,
            cancellationToken
        );

        // Assert
        Assert.Null(result.RejectionReason);
        Assert.Same(sensor, result.ValidatedSensor);

        await _sensorReader.Received(1).GetByIdAsync(
            sensor.Id,
            cancellationToken
        );

        await _deviceReader.Received(1).GetByIdAsync(
            device.Id,
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
