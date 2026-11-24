using NSubstitute;
using SmartHome.Application.Abstractions.Sensors;
using SmartHome.Application.Abstractions.Telemetry;
using SmartHome.Application.UseCases.Telemetry.GetSensorClimateMeasurements;
using SmartHome.Application.UseCases.Telemetry.Shared.Results;
using SmartHome.Application.UseCases.Telemetry.Shared.Validation;
using SmartHome.Domain.Devices;
using SmartHome.Domain.Measurements;
using SmartHome.Domain.Sensors;

namespace SmartHome.Application.Tests.UseCases.Telemetry.GetSensorClimateMeasurements;

public class GetSensorClimateMeasurementsUseCaseTests
{
    private readonly ISensorReader _sensorReader;
    private readonly IClimateMeasurementReader _measurementReader;
    private readonly TelemetryQueryValidator _queryValidator;
    private readonly TelemetryQuerySensorValidator _sensorValidator;
    private readonly GetSensorClimateMeasurementsUseCase _useCase;

    public GetSensorClimateMeasurementsUseCaseTests()
    {
        _sensorReader = Substitute.For<ISensorReader>();
        _measurementReader = Substitute.For<IClimateMeasurementReader>();
        
        _queryValidator = new TelemetryQueryValidator();
        _sensorValidator = new TelemetryQuerySensorValidator(_sensorReader);
        
        _useCase = new GetSensorClimateMeasurementsUseCase(
            _measurementReader,
            _queryValidator,
            _sensorValidator
        );
    }

    [Fact]
    public async Task ExecuteAsync_ReturnsFailure_WhenQueryIsInvalid()
    {
        // Arrange
        GetSensorClimateMeasurementsRequest request = CreateRequest() with
        {
            FromUtc = default
        };

        // Act
        GetSensorClimateMeasurementsResult result = await _useCase.ExecuteAsync(request);

        // Assert
        Assert.Equal(TelemetryQueryOutcome.Failed, result.Outcome);
        Assert.Equal(TelemetryQueryFailureReason.InvalidFromUtc, result.FailureReason);
        Assert.Empty(result.Measurements);

        await _sensorReader.DidNotReceive().GetByIdAsync(
            Arg.Any<SensorId>(),
            Arg.Any<CancellationToken>()
        );

        await _measurementReader.DidNotReceive().GetBySensorIdAsync(
            Arg.Any<SensorId>(),
            Arg.Any<DateTime>(),
            Arg.Any<DateTime>(),
            Arg.Any<int>(),
            Arg.Any<CancellationToken>()
        );
    }

    [Fact]
    public async Task ExecuteAsync_ReturnsFailure_WhenSensorDoesNotExist()
    {
        // Arrange
        GetSensorClimateMeasurementsRequest request = CreateRequest();

        _sensorReader.GetByIdAsync(
            request.SensorId,
            Arg.Any<CancellationToken>()
        ).Returns(Task.FromResult<Sensor?>(null));

        // Act
        GetSensorClimateMeasurementsResult result = await _useCase.ExecuteAsync(request);

        // Assert
        Assert.Equal(TelemetryQueryOutcome.Failed, result.Outcome);
        Assert.Equal(TelemetryQueryFailureReason.SensorNotFound, result.FailureReason);
        Assert.Empty(result.Measurements);

        await _measurementReader.DidNotReceive().GetBySensorIdAsync(
            Arg.Any<SensorId>(),
            Arg.Any<DateTime>(),
            Arg.Any<DateTime>(),
            Arg.Any<int>(),
            Arg.Any<CancellationToken>()
        );
    }

    // TODO:  Add a category-mismatch test when SensorCategory has another valid value.
    // [Fact]
    // public async Task ExecuteAsync_ReturnsFailure_WhenSensorCategoryDoesNotMatch()
    // {
    //     // Arrange
    //     Device device = CreateDevice();
    //     Sensor sensor = new Sensor(
    //         device.Id,
    //         SensorCategory.NOT_CURRENTLY_AVAILABLE,
    //         "Sensor Name",
    //         "Sensor Model"
    //     );

    //     GetSensorClimateMeasurementsRequest request = CreateRequest(sensor.Id);

    //     _sensorReader.GetByIdAsync(
    //         request.SensorId,
    //         Arg.Any<CancellationToken>()
    //     ).Returns(Task.FromResult<Sensor?>(sensor));

    //     // Act
    //     GetSensorClimateMeasurementsResult result = await _useCase.ExecuteAsync(request);

    //     // Assert
    //     Assert.Equal(TelemetryQueryOutcome.Failed, result.Outcome);
    //     Assert.Equal(TelemetryQueryFailureReason.InvalidSensorCategory, result.FailureReason);
    //     Assert.Empty(result.Measurements);

    //     await _measurementReader.DidNotReceive().GetBySensorIdAsync(
    //         Arg.Any<SensorId>(),
    //         Arg.Any<DateTime>(),
    //         Arg.Any<DateTime>(),
    //         Arg.Any<int>(),
    //         Arg.Any<CancellationToken>()
    //     );
    // }

    [Fact]
    public async Task ExecuteAsync_ReturnsSuccessWithMeasurements_WhenQueryAndSensorAreValid()
    {
        // Arrange
        Device device = CreateDevice();
        Sensor sensor = CreateSensor(device.Id);

        GetSensorClimateMeasurementsRequest request = CreateRequest(sensor.Id);

        CancellationToken cancellationToken = new();

        _sensorReader.GetByIdAsync(
            request.SensorId,
            cancellationToken
        ).Returns(Task.FromResult<Sensor?>(sensor));

        IReadOnlyList<ClimateMeasurement> measurements = new List<ClimateMeasurement>
        {
            CreateMeasurement(sensor.Id, 10),
            CreateMeasurement(sensor.Id, 20),
            CreateMeasurement(sensor.Id, 30)
        };        

        _measurementReader.GetBySensorIdAsync(
            request.SensorId,
            request.FromUtc,
            request.ToUtc,
            request.Limit,
            cancellationToken
        ).Returns(Task.FromResult(measurements));

        // Act
        GetSensorClimateMeasurementsResult result = await _useCase.ExecuteAsync(request, cancellationToken);

        // Assert
        Assert.Equal(TelemetryQueryOutcome.Succeeded, result.Outcome);
        Assert.Null(result.FailureReason);
        Assert.Equal(measurements, result.Measurements);

        await _measurementReader.Received(1).GetBySensorIdAsync(
            sensor.Id,
            request.FromUtc,
            request.ToUtc,
            request.Limit,
            cancellationToken
        );
    }

    [Fact]
    public async Task ExecuteAsync_ReturnsSuccessWithEmptyMeasurements_WhenReaderReturnsNoMeasurements()
    {
        // Arrange
        Device device = CreateDevice();
        Sensor sensor = CreateSensor(device.Id);

        GetSensorClimateMeasurementsRequest request = CreateRequest(sensor.Id);

        CancellationToken cancellationToken = new();

        _sensorReader.GetByIdAsync(
            request.SensorId,
            cancellationToken
        ).Returns(Task.FromResult<Sensor?>(sensor));

        IReadOnlyList<ClimateMeasurement> measurements = Array.Empty<ClimateMeasurement>();

        _measurementReader.GetBySensorIdAsync(
            request.SensorId,
            request.FromUtc,
            request.ToUtc,
            request.Limit,
            cancellationToken
        ).Returns(Task.FromResult(measurements));

        // Act
        GetSensorClimateMeasurementsResult result = await _useCase.ExecuteAsync(request, cancellationToken);

        // Assert
        Assert.Equal(TelemetryQueryOutcome.Succeeded, result.Outcome);
        Assert.Null(result.FailureReason);
        Assert.Empty(result.Measurements);

        await _measurementReader.Received(1).GetBySensorIdAsync(
            sensor.Id,
            request.FromUtc,
            request.ToUtc,
            request.Limit,
            cancellationToken
        );
    }

    [Fact]
    public async Task ExecuteAsync_PassesCancellationTokenToDependencies_WhenRequestIsValid()
    {
        // Arrange
        Device device = CreateDevice();
        Sensor sensor = CreateSensor(device.Id);

        GetSensorClimateMeasurementsRequest request = CreateRequest(sensor.Id);

        CancellationToken cancellationToken = new();

        _sensorReader.GetByIdAsync(
            request.SensorId,
            cancellationToken
        ).Returns(Task.FromResult<Sensor?>(sensor));

        IReadOnlyList<ClimateMeasurement> measurements = Array.Empty<ClimateMeasurement>();

        _measurementReader.GetBySensorIdAsync(
            request.SensorId,
            request.FromUtc,
            request.ToUtc,
            request.Limit,
            cancellationToken
        ).Returns(Task.FromResult(measurements));

        // Act
        await _useCase.ExecuteAsync(request, cancellationToken);

        // Assert
        await _sensorReader.Received(1).GetByIdAsync(
            sensor.Id,
            cancellationToken
        );

        await _measurementReader.Received(1).GetBySensorIdAsync(
            sensor.Id,
            request.FromUtc,
            request.ToUtc,
            request.Limit,
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

    private static ClimateMeasurement CreateMeasurement(
        SensorId sensorId,
        int minute)
    {
        DateTime measuredAtUtc = new(
            2026, 6, 6, 12, minute, 0, DateTimeKind.Utc
        );

        return new ClimateMeasurement(
            new SourceMessageId(minute),
            sensorId,
            measuredAtUtc,
            measuredAtUtc.AddMinutes(1),
            21.5m,
            50m
        );
    }

    private static GetSensorClimateMeasurementsRequest CreateRequest(
        SensorId? sensorId = null,
        DateTime? fromUtc = null,
        DateTime? toUtc = null,
        int limit = 100)
    {
        return new GetSensorClimateMeasurementsRequest(
            sensorId ?? SensorId.New(),
            fromUtc ?? new DateTime(2026, 6, 6, 12, 0, 0, DateTimeKind.Utc),
            toUtc ?? new DateTime(2026, 6, 6, 13, 0, 0, DateTimeKind.Utc),
            limit
        );
    }    
}
