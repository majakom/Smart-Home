using MQTTnet;

namespace SmartHome.TelemetryWorker.Mqtt.Receiving;

public interface IMqttMessageSubscriber
{
    void RegisterMessageHandler(
        IMqttClient mqttClient,
        CancellationToken cancellationToken
    );

    Task SubscribeAsync(
        IMqttClient mqttClient,
        CancellationToken cancellationToken
    );
}
