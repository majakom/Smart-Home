using System.Text.Json;
using SmartHome.Application.UseCases.Telemetry.RecordClimateMeasurement;
using SmartHome.Domain.Measurements;
using SmartHome.Domain.Sensors;
using SmartHome.TelemetryWorker.Mqtt.Contracts.Incoming.Telemetry;
using SmartHome.TelemetryWorker.Mqtt.Topics.Telemetry;

namespace SmartHome.TelemetryWorker.Mqtt.Receiving.Telemetry.Handlers;

public sealed class ClimateMeasurementMessageHandler : ITelemetryMessageHandler
{
    private readonly RecordClimateMeasurementUseCase _useCase;
    private readonly TelemetryOutcomeLogger _outcomeLogger;
    ILogger<ClimateMeasurementMessageHandler> _logger;

    public ClimateMeasurementMessageHandler(
        RecordClimateMeasurementUseCase useCase,
        TelemetryOutcomeLogger outcomeLogger,
        ILogger<ClimateMeasurementMessageHandler> logger
    )
    {
        _useCase = useCase;
        _outcomeLogger = outcomeLogger;
        _logger = logger;
    }

    public SensorCategory Category => SensorCategory.Climate;

    public async Task HandleAsync(
        TelemetryTopic topic,
        ReadOnlyMemory<byte> payload,
        CancellationToken cancellationToken
    )
    {
        ClimateMeasurementMessage? message;
        try
        {
            message = JsonSerializer.Deserialize<ClimateMeasurementMessage>(payload.Span);
        }
        catch (JsonException exception)
        {
            _logger.LogWarning(
                exception,
                "Ignoring climate telemetry message with invalid JSON from sensor {SensorId}.",
                topic.SensorId
            );
            return;
        }
         
        if (message is null)
        {
            _logger.LogWarning(
                "Ignoring empty climate telemetry message from sensor {SensorId}.",
                topic.SensorId
            );
            return;
        }

        RecordClimateMeasurementRequest request = new(
            SourceMessageId: new SourceMessageId(message.SourceMessageId),
            SensorId: topic.SensorId,
            DeviceId: topic.DeviceId,
            MeasuredAtUtc: message.MeasuredAtUtc.UtcDateTime,
            TemperatureCelsius: message.TemperatureCelsius,
            RelativeHumidityPercent: message.RelativeHumidityPercent
        );

        RecordClimateMeasurementResult result = await _useCase.ExecuteAsync(request, cancellationToken);

        _outcomeLogger.Log(
            result.Outcome,
            result.RejectionReason,
            Category.ToString(),
            topic.SensorId.ToString()
        );
    }
}
