using SmartHome.Domain.Sensors;
using SmartHome.TelemetryWorker.Mqtt.Topics.Telemetry;

namespace SmartHome.TelemetryWorker.Mqtt.Receiving.Telemetry.Handlers;

public interface ITelemetryMessageHandler
{
    SensorCategory Category { get; }

    Task HandleAsync(
        TelemetryTopic topic,
        ReadOnlyMemory<byte> payload,
        CancellationToken cancellationToken
    );
}
