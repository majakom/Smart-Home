using NSubstitute;
using SmartHome.Application.Abstractions.Devices;
using SmartHome.Application.Abstractions.Sensors;
using SmartHome.Application.Abstractions.Telemetry;
using SmartHome.Application.UseCases.Telemetry.RecordClimateMeasurement;
using SmartHome.Application.UseCases.Telemetry.Shared.Results;
using SmartHome.Application.UseCases.Telemetry.Shared.Validation;
using SmartHome.Domain.Devices;
using SmartHome.Domain.Measurements;
using SmartHome.Domain.Sensors;

namespace SmartHome.Application.Tests.UseCases.Telemetry.RecordClimateMeasurement;

public class RecordClimateMeasurementUseCaseTests
{
    private readonly ISensorReader _sensorReader;
    private readonly IDeviceReader _deviceReader;
    private readonly IClimateMeasurementWriter _measurementWriter;
    private readonly TimeProvider _timeProvider;
    private readonly TelemetrySourceValidator _telemetrySourceValidator;
    private readonly RecordClimateMeasurementRequestValidator _requestValidator;
    private readonly RecordClimateMeasurementUseCase _useCase;

    public RecordClimateMeasurementUseCaseTests()
    {
        _sensorReader = Substitute.For<ISensorReader>();
        _deviceReader = Substitute.For<IDeviceReader>();
        _measurementWriter = Substitute.For<IClimateMeasurementWriter>();

        _timeProvider = new FixedTimeProvider(
            new DateTimeOffset(2026, 6, 6, 12, 0, 0, TimeSpan.Zero)
        );

        _telemetrySourceValidator = new TelemetrySourceValidator(_sensorReader, _deviceReader);
        _requestValidator = new RecordClimateMeasurementRequestValidator(new TelemetryRequestValidator());

        _useCase = new RecordClimateMeasurementUseCase(
            _telemetrySourceValidator,
            _requestValidator,
            _measurementWriter,
            _timeProvider
        );
    }

    [Fact]
    public async Task ExecuteAsync_ReturnsRejected_WhenRequestIsInvalid()
    {
        // Arrange
        RecordClimateMeasurementRequest request = CreateRequest(
            sourceMessageId: new SourceMessageId(0)
        );

        // Act
        RecordClimateMeasurementResult result = await _useCase.ExecuteAsync(request);

        // Assert
        Assert.Equal(TelemetryOutcome.Rejected, result.Outcome);
        Assert.Equal(TelemetryRejectionReason.InvalidSourceMessageId, result.RejectionReason);
        Assert.Null(result.MeasurementId);

        await _sensorReader.DidNotReceive().GetByIdAsync(
            Arg.Any<SensorId>(),
            Arg.Any<CancellationToken>()
        );

        await _deviceReader.DidNotReceive().GetByIdAsync(
            Arg.Any<DeviceId>(),
            Arg.Any<CancellationToken>()
        );

        await _measurementWriter.DidNotReceive().TryAddAsync(
            Arg.Any<ClimateMeasurement>(),
            Arg.Any<CancellationToken>()
        );
    }

    [Fact]
    public async Task ExecuteAsync_ReturnsRejected_WhenTelemetrySourceIsInvalid()
    {
        // Arrange
        RecordClimateMeasurementRequest request = CreateRequest();

        _sensorReader.GetByIdAsync(
            request.SensorId,
            Arg.Any<CancellationToken>()
        ).Returns(Task.FromResult<Sensor?>(null));

        // Act
        RecordClimateMeasurementResult result = await _useCase.ExecuteAsync(request);

        // Assert
        Assert.Equal(TelemetryOutcome.Rejected, result.Outcome);
        Assert.Equal(TelemetryRejectionReason.SensorNotFound, result.RejectionReason);
        Assert.Null(result.MeasurementId);

        await _measurementWriter.DidNotReceive().TryAddAsync(
            Arg.Any<ClimateMeasurement>(),
            Arg.Any<CancellationToken>()
        );
    }

    [Fact]
    public async Task ExecuteAsync_ReturnsDuplicate_WhenMeasurementAlreadyExists()
    {
        // Arrange
        Device device = CreateDevice();
        Sensor sensor = CreateSensor(device.Id);

        RecordClimateMeasurementRequest request = CreateRequest(
            deviceId: device.Id,
            sensorId: sensor.Id
        );
        ConfigureValidSource(device, sensor);

        _measurementWriter.TryAddAsync(
            Arg.Any<ClimateMeasurement>(),
            Arg.Any<CancellationToken>()
        ).Returns(Task.FromResult(false));

        // Act
        RecordClimateMeasurementResult result = await _useCase.ExecuteAsync(request);

        // Assert
        Assert.Equal(TelemetryOutcome.Duplicate, result.Outcome);
        Assert.Null(result.RejectionReason);
        Assert.Null(result.MeasurementId);

        await _measurementWriter.Received(1).TryAddAsync(
            Arg.Any<ClimateMeasurement>(),
            Arg.Any<CancellationToken>()
        );
    }

    [Fact]
    public async Task ExecuteAsync_ReturnsRecorded_WhenMeasurementIsSuccessfullyAdded()
    {
        // Arrange
        Device device = CreateDevice();
        Sensor sensor = CreateSensor(device.Id);

        RecordClimateMeasurementRequest request = CreateRequest(
            deviceId: device.Id,
            sensorId: sensor.Id
        );
        ConfigureValidSource(device, sensor);

        ClimateMeasurement? writtenMeasurement = null;
        CancellationToken cancellationToken = new();

        _measurementWriter.TryAddAsync(
            Arg.Do<ClimateMeasurement>(measurement =>
                writtenMeasurement = measurement),
            cancellationToken
        ).Returns(Task.FromResult(true));

        DateTime expectedReceivedAtUtc = _timeProvider.GetUtcNow().UtcDateTime;

        // Act
        RecordClimateMeasurementResult result = await _useCase.ExecuteAsync(request, cancellationToken);

        // Assert
        Assert.Equal(TelemetryOutcome.Recorded, result.Outcome);
        Assert.Null(result.RejectionReason);
        Assert.NotNull(result.MeasurementId);

        ClimateMeasurement measurement = Assert.IsType<ClimateMeasurement>(
            writtenMeasurement
        );
        Assert.Equal(result.MeasurementId, measurement.Id);
        Assert.Equal(request.SourceMessageId, measurement.SourceMessageId);
        Assert.Equal(request.SensorId, measurement.SensorId);
        Assert.Equal(request.MeasuredAtUtc, measurement.MeasuredAtUtc);
        Assert.Equal(expectedReceivedAtUtc, measurement.ReceivedAtUtc);
        Assert.Equal(request.TemperatureCelsius, measurement.TemperatureCelsius);
        Assert.Equal(request.RelativeHumidityPercent, measurement.RelativeHumidityPercent);

        await _measurementWriter.Received(1).TryAddAsync(
            Arg.Any<ClimateMeasurement>(),
            cancellationToken
        );
    }

    [Fact]
    public async Task ExecuteAsync_PassesCancellationTokenToDependencies_WhenRequestIsValid()
    {
        // Arrange
        Device device = CreateDevice();
        Sensor sensor = CreateSensor(device.Id);

        RecordClimateMeasurementRequest request = CreateRequest(
            deviceId: device.Id,
            sensorId: sensor.Id
        );
        ConfigureValidSource(device, sensor);

        _measurementWriter.TryAddAsync(
            Arg.Any<ClimateMeasurement>(),
            Arg.Any<CancellationToken>()
        ).Returns(Task.FromResult(true));

        CancellationToken cancellationToken = new();

        // Act
        await _useCase.ExecuteAsync(request, cancellationToken);

        // Assert
        await _sensorReader.Received(1).GetByIdAsync(
            sensor.Id,
            cancellationToken
        );

        await _deviceReader.Received(1).GetByIdAsync(
            device.Id,
            cancellationToken
        );

        await _measurementWriter.Received(1).TryAddAsync(
            Arg.Any<ClimateMeasurement>(),
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

    private void ConfigureValidSource(Device device, Sensor sensor)
    {
        _sensorReader.GetByIdAsync(
            sensor.Id,
            Arg.Any<CancellationToken>()
        ).Returns(Task.FromResult<Sensor?>(sensor));

        _deviceReader.GetByIdAsync(
            device.Id,
            Arg.Any<CancellationToken>()
        ).Returns(Task.FromResult<Device?>(device));
    }

    private sealed class FixedTimeProvider : TimeProvider
    {
        private readonly DateTimeOffset _utcNow;

        public FixedTimeProvider(DateTimeOffset utcNow)
        {
            _utcNow = utcNow;
        }

        public override DateTimeOffset GetUtcNow()
        {
            return _utcNow;
        }
    }
}
