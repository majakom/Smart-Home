using MQTTnet;
using MQTTnet.Packets;
using MQTTnet.Protocol;
using SmartHome.Domain.Sensors;
using SmartHome.TelemetryWorker.Mqtt.Topics.Telemetry;

namespace SmartHome.TelemetryWorker.Mqtt.Receiving.Telemetry;

public sealed class TelemetrySubscriber
{
    public void RegisterMessageHandler(
        IMqttClient mqttClient,
        Func<MqttApplicationMessageReceivedEventArgs, Task> handler
    )
    {
        ArgumentNullException.ThrowIfNull(mqttClient);
        ArgumentNullException.ThrowIfNull(handler);

        mqttClient.ApplicationMessageReceivedAsync += handler;
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
