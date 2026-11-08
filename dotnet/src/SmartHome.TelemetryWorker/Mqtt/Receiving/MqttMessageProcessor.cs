using MQTTnet;

namespace SmartHome.TelemetryWorker.Mqtt.Receiving;

public class MqttMessageProcessor(
    IEnumerable<IMqttMessageSubscriber> subscribers
)
{
    public void RegisterHandlers(
        IMqttClient mqttClient,
        CancellationToken cancellationToken
    )
    {
        ArgumentNullException.ThrowIfNull(mqttClient);

        foreach (IMqttMessageSubscriber subscriber in subscribers)
        {
            subscriber.RegisterMessageHandler(mqttClient, cancellationToken);
        }
    }

    public async Task SubscribeAsync(
        IMqttClient client,
        CancellationToken cancellationToken
    )
    {
        ArgumentNullException.ThrowIfNull(client);

        foreach (IMqttMessageSubscriber subscriber in subscribers)
        {
            await subscriber.SubscribeAsync(
                client,
                cancellationToken
            );
        }
    }
}
