using System.Buffers;
using MQTTnet;
using MQTTnet.Packets;
using MQTTnet.Protocol;
using SmartHome.Domain.Sensors;
using SmartHome.TelemetryWorker.Mqtt.Topics.Telemetry;

namespace SmartHome.TelemetryWorker.Mqtt.Receiving.Telemetry;

public sealed class TelemetrySubscriber(
    TelemetryMessageDispatcher dispatcher
) : IMqttMessageSubscriber
{
    public void RegisterMessageHandler(
        IMqttClient mqttClient,
        CancellationToken cancellationToken
    )
    {
        ArgumentNullException.ThrowIfNull(mqttClient);

        mqttClient.ApplicationMessageReceivedAsync += args =>
            dispatcher.DispatchAsync(
                args.ApplicationMessage.Topic,
                args.ApplicationMessage.Payload.ToArray(),
                cancellationToken
            );
    }

    public async Task SubscribeAsync(
        IMqttClient mqttClient,
        CancellationToken cancellationToken
    )
    {
        ArgumentNullException.ThrowIfNull(mqttClient);
        
        if (!mqttClient.IsConnected)
            throw new InvalidOperationException("Cannot subscribe because the MQTT client is not connected.");

        MqttTopicFilter topicFilter = new MqttTopicFilterBuilder()
            .WithTopic(TelemetryTopicFilters.ForCategory(SensorCategory.Climate))
            .WithQualityOfServiceLevel(MqttQualityOfServiceLevel.AtLeastOnce)
            .Build();

        MqttClientSubscribeOptions options = new MqttClientSubscribeOptionsBuilder()
            .WithTopicFilter(topicFilter)
            .Build();

        await mqttClient.SubscribeAsync(options, cancellationToken);
    }
}
