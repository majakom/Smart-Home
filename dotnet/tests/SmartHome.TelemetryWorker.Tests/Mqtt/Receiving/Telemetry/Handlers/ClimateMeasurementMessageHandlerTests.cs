using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using NSubstitute;
using SmartHome.Application.Abstractions.Devices;
using SmartHome.Application.Abstractions.Sensors;
using SmartHome.Application.Abstractions.Telemetry;
using SmartHome.Application.UseCases.Telemetry.RecordClimateMeasurement;
using SmartHome.Application.UseCases.Telemetry.Shared.Validation;
using SmartHome.Domain.Devices;
using SmartHome.Domain.Measurements;
using SmartHome.Domain.Sensors;
using SmartHome.TelemetryWorker.Mqtt.Receiving.Telemetry;
using SmartHome.TelemetryWorker.Mqtt.Receiving.Telemetry.Handlers;
using SmartHome.TelemetryWorker.Mqtt.Topics.Telemetry;

namespace SmartHome.TelemetryWorker.Tests.Mqtt.Receiving.Telemetry.Handlers;

public class ClimateMeasurementMessageHandlerTests
{
    private const string ValidPayload =
        """{"sourceMessageId":42,"measuredAtUtc":"2026-06-06T12:00:00Z","temperatureCelsius":21.5,"relativeHumidityPercent":48.25}""";

    [Fact]
    public void Category_ReturnsClimate()
    {
        HandlerFixture fixture = new();

        Assert.Equal(SensorCategory.Climate, fixture.Handler.Category);
    }

    [Fact]
    public async Task HandleAsync_MapsMessageToMeasurement_AndPassesCancellationToken()
    {
        // Arrange
        HandlerFixture fixture = new();
        using CancellationTokenSource cancellationSource = new();

        const string payload = ValidPayload;

        // Act
        await fixture.Handler.HandleAsync(
            fixture.Topic,
            Encoding.UTF8.GetBytes(payload),
            cancellationSource.Token
        );

        // Assert
        await fixture.Writer.Received(1).TryAddAsync(
            Arg.Any<ClimateMeasurement>(),
            cancellationSource.Token
        );

        ClimateMeasurement measurement = Assert.IsType<ClimateMeasurement>(fixture.WrittenMeasurement);

        Assert.Equal(new SourceMessageId(42), measurement.SourceMessageId);
        Assert.Equal(fixture.Sensor.Id, measurement.SensorId);
        Assert.Equal(
            new DateTime(2026, 6, 6, 12, 0, 0, DateTimeKind.Utc),
            measurement.MeasuredAtUtc
        );
        Assert.Equal(21.5m, measurement.TemperatureCelsius);
        Assert.Equal(48.25m, measurement.RelativeHumidityPercent);

        RecordedLog outcomeLog = Assert.Single(fixture.OutcomeLogger.Entries);
        Assert.Equal(LogLevel.Information, outcomeLog.Level);
        Assert.Contains("Recorded", outcomeLog.Message);
        Assert.Contains("Climate", outcomeLog.Message);
        Assert.Contains(fixture.Sensor.Id.ToString(), outcomeLog.Message);
    }

    [Fact]
    public async Task HandleAsync_LogsDuplicateOutcome_WhenMeasurementAlreadyExists()
    {
        // Arrange
        HandlerFixture fixture = new(writerAddsMeasurement: false);

        const string payload = ValidPayload;

        // Act
        await fixture.Handler.HandleAsync(
            fixture.Topic,
            Encoding.UTF8.GetBytes(payload),
            CancellationToken.None
        );

        // Assert
        await fixture.Writer.Received(1).TryAddAsync(
            Arg.Any<ClimateMeasurement>(),
            Arg.Any<CancellationToken>()
        );

        RecordedLog outcomeLog = Assert.Single(fixture.OutcomeLogger.Entries);
        Assert.Equal(LogLevel.Debug, outcomeLog.Level);
        Assert.Contains("Ignored", outcomeLog.Message);
        Assert.Contains("Climate", outcomeLog.Message);
        Assert.Contains(fixture.Sensor.Id.ToString(), outcomeLog.Message);
    }

    [Fact]
    public async Task HandleAsync_LogsRejectedOutcome_WhenSensorDoesNotExist()
    {
        // Arrange
        HandlerFixture fixture = new(sensorExists: false);

        const string payload = ValidPayload;

        // Act
        await fixture.Handler.HandleAsync(
            fixture.Topic,
            Encoding.UTF8.GetBytes(payload),
            CancellationToken.None
        );

        // Assert
        await fixture.Writer.DidNotReceive().TryAddAsync(
            Arg.Any<ClimateMeasurement>(),
            Arg.Any<CancellationToken>()
        );

        RecordedLog outcomeLog = Assert.Single(fixture.OutcomeLogger.Entries);
        Assert.Equal(LogLevel.Warning, outcomeLog.Level);
        Assert.Contains("Rejected", outcomeLog.Message);
        Assert.Contains("SensorNotFound", outcomeLog.Message);
    }

    [Fact]
    public async Task HandleAsync_DoesNotWriteMeasurement_WhenJsonIsInvalid()
    {
        // Arrange
        HandlerFixture fixture = new();

        // Act
        await fixture.Handler.HandleAsync(
            fixture.Topic,
            Encoding.UTF8.GetBytes("{invalid json"),
            CancellationToken.None
        );

        // Assert
        await fixture.Writer.DidNotReceive().TryAddAsync(
            Arg.Any<ClimateMeasurement>(),
            Arg.Any<CancellationToken>()
        );

        RecordedLog warningLog = Assert.Single(fixture.HandlerLogger.Entries);
        Assert.Equal(LogLevel.Warning, warningLog.Level);
        Assert.IsType<JsonException>(warningLog.Exception);
        Assert.Contains("invalid JSON", warningLog.Message);
        Assert.Contains(fixture.Sensor.Id.ToString(), warningLog.Message);
    }

    [Fact]
    public async Task HandleAsync_DoesNotWriteMeasurement_WhenJsonIsNull()
    {
        // Arrange
        HandlerFixture fixture = new();

        // Act
        await fixture.Handler.HandleAsync(
            fixture.Topic,
            Encoding.UTF8.GetBytes("null"),
            CancellationToken.None
        );

        // Assert
        await fixture.Writer.DidNotReceive().TryAddAsync(
            Arg.Any<ClimateMeasurement>(),
            Arg.Any<CancellationToken>()
        );

        RecordedLog warningLog = Assert.Single(fixture.HandlerLogger.Entries);
        Assert.Equal(LogLevel.Warning, warningLog.Level);
        Assert.Null(warningLog.Exception);
        Assert.Contains("empty climate telemetry message", warningLog.Message);
        Assert.Contains(fixture.Sensor.Id.ToString(), warningLog.Message);
    }

    [Fact]
    public async Task HandleAsync_DoesNotWriteMeasurement_WhenRequiredPropertyIsMissing()
    {
        // Arrange
        HandlerFixture fixture = new();

        const string payload =
            """{"sourceMessageId":42,"measuredAtUtc":"2026-06-06T12:00:00Z","relativeHumidityPercent":48.25}""";

        // Act
        await fixture.Handler.HandleAsync(
            fixture.Topic,
            Encoding.UTF8.GetBytes(payload),
            CancellationToken.None
        );

        // Assert
        await fixture.Writer.DidNotReceive().TryAddAsync(
            Arg.Any<ClimateMeasurement>(),
            Arg.Any<CancellationToken>()
        );

        RecordedLog warningLog = Assert.Single(fixture.HandlerLogger.Entries);
        Assert.Equal(LogLevel.Warning, warningLog.Level);
        Assert.IsType<JsonException>(warningLog.Exception);
        Assert.Contains("invalid JSON", warningLog.Message);
        Assert.Contains(fixture.Sensor.Id.ToString(), warningLog.Message);
    }

    private sealed class HandlerFixture
    {
        public Device Device { get; }
        public Sensor Sensor { get; }
        public TelemetryTopic Topic { get; }
        public IClimateMeasurementWriter Writer { get; }
        public ClimateMeasurement? WrittenMeasurement { get; private set; }
        public RecordingLogger<TelemetryOutcomeLogger> OutcomeLogger { get; }
        public RecordingLogger<ClimateMeasurementMessageHandler> HandlerLogger { get; }
        public ClimateMeasurementMessageHandler Handler { get; }

        public HandlerFixture(
            bool sensorExists = true,
            bool writerAddsMeasurement = true)
        {
            Device = new Device("Device Name", "Device Model");
            Sensor = new Sensor(
                Device.Id,
                SensorCategory.Climate,
                "Sensor Name",
                "Sensor Model"
            );
            Topic = new TelemetryTopic(
                Device.Id,
                Sensor.Id,
                SensorCategory.Climate
            );
            Writer = Substitute.For<IClimateMeasurementWriter>();
            OutcomeLogger = new RecordingLogger<TelemetryOutcomeLogger>();
            HandlerLogger = new RecordingLogger<ClimateMeasurementMessageHandler>();

            ISensorReader sensorReader = Substitute.For<ISensorReader>();
            sensorReader.GetByIdAsync(
                Sensor.Id,
                Arg.Any<CancellationToken>())
                .Returns(Task.FromResult<Sensor?>(sensorExists ? Sensor : null));

            IDeviceReader deviceReader = Substitute.For<IDeviceReader>();
            deviceReader.GetByIdAsync(
                Device.Id,
                Arg.Any<CancellationToken>())
                .Returns(Task.FromResult<Device?>(Device));

            Writer.TryAddAsync(
                Arg.Do<ClimateMeasurement>(
                    measurement => WrittenMeasurement = measurement),
                Arg.Any<CancellationToken>())
                .Returns(Task.FromResult(writerAddsMeasurement));

            RecordClimateMeasurementUseCase useCase = new(
                new TelemetrySourceValidator(sensorReader, deviceReader),
                new RecordClimateMeasurementRequestValidator(
                    new TelemetryRequestValidator()),
                Writer,
                TimeProvider.System
            );

            Handler = new ClimateMeasurementMessageHandler(
                useCase,
                new TelemetryOutcomeLogger(OutcomeLogger),
                HandlerLogger
            );
        }
    }

    private sealed class RecordingLogger<T> : ILogger<T>
    {
        public List<RecordedLog> Entries { get; } = [];

        public IDisposable? BeginScope<TState>(TState state)
            where TState : notnull
        {
            return null;
        }

        public bool IsEnabled(LogLevel logLevel) => true;

        public void Log<TState>(
            LogLevel logLevel,
            EventId eventId,
            TState state,
            Exception? exception,
            Func<TState, Exception?, string> formatter)
        {
            Entries.Add(
                new RecordedLog(
                    logLevel,
                    formatter(state, exception),
                    exception
                )
            );
        }
    }

    private sealed record RecordedLog(
        LogLevel Level,
        string Message,
        Exception? Exception
    );
}
