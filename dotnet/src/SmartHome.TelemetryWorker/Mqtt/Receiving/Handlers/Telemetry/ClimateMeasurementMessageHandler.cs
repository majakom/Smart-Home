using SmartHome.Application.UseCases.Telemetry.RecordClimateMeasurement;
using SmartHome.Domain.Measurements;
using SmartHome.TelemetryWorker.Mqtt.Contracts.Incoming.Telemetry;
using SmartHome.TelemetryWorker.Mqtt.Topics.Telemetry;

namespace SmartHome.TelemetryWorker.Mqtt.Receiving.Handlers.Telemetry;

public sealed class ClimateMeasurementMessageHandler
{
    private readonly RecordClimateMeasurementUseCase _useCase;

    public ClimateMeasurementMessageHandler(
        RecordClimateMeasurementUseCase useCase
    )
    {
        _useCase = useCase;
    }

    public Task<RecordClimateMeasurementResult> HandleAsync(
        TelemetryTopic topic,
        ClimateMeasurementMessage message,
        CancellationToken cancellationToken
    )
    {
        RecordClimateMeasurementRequest request = new(
            new SourceMessageId(message.SourceMessageId),
            topic.SensorId,
            topic.DeviceId,
            message.MeasuredAtUtc.UtcDateTime,
            message.TemperatureCelsius,
            message.RelativeHumidityPercent
        );

        return _useCase.ExecuteAsync(request, cancellationToken);
    }
}
