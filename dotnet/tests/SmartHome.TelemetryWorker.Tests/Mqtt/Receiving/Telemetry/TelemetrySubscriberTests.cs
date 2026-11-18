
using System.Text;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using MQTTnet;
using MQTTnet.Packets;
using MQTTnet.Protocol;
using NSubstitute;
using SmartHome.Domain.Devices;
using SmartHome.Domain.Sensors;
using SmartHome.TelemetryWorker.Mqtt.Receiving.Telemetry;
using SmartHome.TelemetryWorker.Mqtt.Receiving.Telemetry.Handlers;
using SmartHome.TelemetryWorker.Mqtt.Topics.Telemetry;

namespace SmartHome.TelemetryWorker.Tests.Mqtt.Receiving.Telemetry;

public class TelemetrySubscriberTests
{
    [Fact]
    public async Task RegisterMessageHandler_ForwardsMessageToDispatcher()
    {
        // Arrange
        Guid deviceId = Guid.NewGuid();
        Guid sensorId = Guid.NewGuid();

        string topic =
            $"smarthome/devices/{deviceId:D}/sensors/{sensorId:D}/telemetry/climate";

        byte[] payload = Encoding.UTF8.GetBytes("""{"temperatureCelsius":21.5}""");

        ITelemetryMessageHandler handler = Substitute.For<ITelemetryMessageHandler>();
        handler.Category.Returns(SensorCategory.Climate);

        ServiceCollection services = new();
        services.AddSingleton(handler);

        using ServiceProvider provider = services.BuildServiceProvider();

        TelemetryMessageDispatcher dispatcher = new(
            provider.GetRequiredService<IServiceScopeFactory>(),
            NullLogger<TelemetryMessageDispatcher>.Instance
        );

        TelemetrySubscriber subscriber = new(dispatcher);

        IMqttClient mqttClient = Substitute.For<IMqttClient>();

        using CancellationTokenSource cancellationSource = new();

        MqttApplicationMessage applicationMessage = new MqttApplicationMessageBuilder()
            .WithTopic(topic)
            .WithPayload(payload)
            .Build();

        MqttPublishPacket publishPacket = new();

        MqttApplicationMessageReceivedEventArgs eventArgs = new(
            "test-client",
            applicationMessage,
            publishPacket,
            (_, _) => Task.CompletedTask
        );

        TelemetryTopic expectedTopic = new(
            new DeviceId(deviceId),
            new SensorId(sensorId),
            SensorCategory.Climate
        );

        // Act
        subscriber.RegisterMessageHandler(
            mqttClient,
            cancellationSource.Token
        );

        mqttClient.ApplicationMessageReceivedAsync +=
            Raise.Event<Func<MqttApplicationMessageReceivedEventArgs, Task>>(
                eventArgs
            );

        // Assert
        await handler.Received(1).HandleAsync(
            expectedTopic,
            Arg.Is<ReadOnlyMemory<byte>>(actual =>
                actual.ToArray().SequenceEqual(payload)),
            cancellationSource.Token
        );
    }

    [Fact]
    public async Task SubscribeAsync_UsesClimateFilterAndAtLeastOnceQos()
    {
        // Arrange
        IMqttClient mqttClient = Substitute.For<IMqttClient>();
        mqttClient.IsConnected.Returns(true);

        MqttClientSubscribeOptions? actualOptions = null;

        mqttClient.SubscribeAsync(
            Arg.Do<MqttClientSubscribeOptions>(
                options => actualOptions = options
            ),
            Arg.Any<CancellationToken>()
        ).Returns(Task.FromResult<MqttClientSubscribeResult>(null!));

        TelemetryMessageDispatcher dispatcher = new(
            Substitute.For<IServiceScopeFactory>(),
            NullLogger<TelemetryMessageDispatcher>.Instance
        );

        TelemetrySubscriber subscriber = new(dispatcher);

        using CancellationTokenSource cancellationSource = new();

        // Act
        await subscriber.SubscribeAsync(
            mqttClient,
            cancellationSource.Token
        );

        // Assert
        await mqttClient.Received(1).SubscribeAsync(
            Arg.Any<MqttClientSubscribeOptions>(),
            cancellationSource.Token
        );

        Assert.NotNull(actualOptions);

        var topicFilter = Assert.Single(actualOptions!.TopicFilters);

        Assert.Equal(
            TelemetryTopicFilters.ForCategory(SensorCategory.Climate),
            topicFilter.Topic
        );

        Assert.Equal(
            MqttQualityOfServiceLevel.AtLeastOnce,
            topicFilter.QualityOfServiceLevel
        );
    }

    [Fact]
    public async Task SubscribeAsync_ThrowsWhenClientIsDisconnected()
    {
        // Arrange
        IMqttClient mqttClient = Substitute.For<IMqttClient>();
        mqttClient.IsConnected.Returns(false);

        TelemetryMessageDispatcher dispatcher = new(
            Substitute.For<IServiceScopeFactory>(),
            NullLogger<TelemetryMessageDispatcher>.Instance
        );

        TelemetrySubscriber subscriber = new(dispatcher);

        // Act
        Func<Task> act = () => subscriber.SubscribeAsync(mqttClient, CancellationToken.None);

        // Assert
        InvalidOperationException exception = await Assert.ThrowsAsync<InvalidOperationException>(act);

        Assert.Contains("not connected", exception.Message);

        await mqttClient.DidNotReceive().SubscribeAsync(
            Arg.Any<MqttClientSubscribeOptions>(),
            Arg.Any<CancellationToken>()
        );
    }
}
