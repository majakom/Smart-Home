using System.Text;
using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Containers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using MQTTnet;
using MQTTnet.Protocol;
using NSubstitute;
using SmartHome.Domain.Devices;
using SmartHome.Domain.Sensors;
using SmartHome.TelemetryWorker.Mqtt.Receiving.Telemetry;
using SmartHome.TelemetryWorker.Mqtt.Receiving.Telemetry.Handlers;
using SmartHome.TelemetryWorker.Mqtt.Topics.Telemetry;
using MqttClientFactory =
    SmartHome.TelemetryWorker.Mqtt.Connection.MqttClientFactory;

namespace SmartHome.TelemetryWorker.Tests.Integration;

public sealed class MqttCommunicationTests : IAsyncLifetime
{
    private readonly IContainer _broker = new ContainerBuilder("eclipse-mosquitto:latest")
        .WithPortBinding(1883, true)
        .WithEntrypoint("sh")
        .WithCommand(
            "-c",
            "printf 'listener 1883 0.0.0.0\\nallow_anonymous true\\n' > /tmp/mosquitto.conf && exec mosquitto -c /tmp/mosquitto.conf")
        .WithWaitStrategy(
            Wait.ForUnixContainer().UntilInternalTcpPortIsAvailable(1883))
        .Build();

    public Task InitializeAsync() => _broker.StartAsync();

    public async Task DisposeAsync() => await _broker.DisposeAsync();

    [Fact]
    public async Task PublishAsync_DeliversTelemetryToMatchingHandler()
    {
        // Arrange
        Guid deviceId = Guid.NewGuid();
        Guid sensorId = Guid.NewGuid();
        string topic =
            $"smarthome/devices/{deviceId:D}/sensors/{sensorId:D}/telemetry/climate";
        byte[] payload = Encoding.UTF8.GetBytes("""{"temperatureCelsius":21.5}""");

        TaskCompletionSource<(TelemetryTopic Topic, byte[] Payload)> received =
            new(TaskCreationOptions.RunContinuationsAsynchronously);

        ITelemetryMessageHandler handler = Substitute.For<ITelemetryMessageHandler>();
        handler.Category.Returns(SensorCategory.Climate);
        handler.HandleAsync(
            Arg.Any<TelemetryTopic>(),
            Arg.Any<ReadOnlyMemory<byte>>(),
            Arg.Any<CancellationToken>()
        )
        .Returns(call =>
            {
                received.TrySetResult(
                    (
                        call.Arg<TelemetryTopic>(),
                        call.Arg<ReadOnlyMemory<byte>>().ToArray()
                    )
                );
                return Task.CompletedTask;
            });

        ServiceCollection services = new();
        services.AddSingleton(handler);
        using ServiceProvider provider = services.BuildServiceProvider();

        TelemetryMessageDispatcher dispatcher = new(
            provider.GetRequiredService<IServiceScopeFactory>(),
            NullLogger<TelemetryMessageDispatcher>.Instance
        );
        TelemetrySubscriber subscriber = new(dispatcher);

        MqttClientFactory clientFactory = new();
        using IMqttClient subscriberClient = clientFactory.CreateClient();
        using IMqttClient publisherClient = clientFactory.CreateClient();

        MqttClientOptions CreateOptions(string clientId) =>
            new MqttClientOptionsBuilder()
                .WithClientId(clientId)
                .WithTcpServer(
                    _broker.Hostname,
                    _broker.GetMappedPublicPort(1883)
                )
                .Build();

        subscriber.RegisterMessageHandler(
            subscriberClient,
            CancellationToken.None
        );

        // Act
        await subscriberClient.ConnectAsync(
            CreateOptions($"subscriber-{Guid.NewGuid():N}"),
            CancellationToken.None
        );
        await subscriber.SubscribeAsync(
            subscriberClient,
            CancellationToken.None
        );

        await publisherClient.ConnectAsync(
            CreateOptions($"publisher-{Guid.NewGuid():N}"),
            CancellationToken.None
        );
        await publisherClient.PublishAsync(
            new MqttApplicationMessageBuilder()
                .WithTopic(topic)
                .WithPayload(payload)
                .WithQualityOfServiceLevel(
                    MqttQualityOfServiceLevel.AtLeastOnce)
                .Build()
        );

        (TelemetryTopic actualTopic, byte[] actualPayload) =
            await received.Task.WaitAsync(TimeSpan.FromSeconds(10));

        // Assert
        Assert.Equal(
            new TelemetryTopic(
                new DeviceId(deviceId),
                new SensorId(sensorId),
                SensorCategory.Climate
            ),
            actualTopic
        );
        Assert.Equal(payload, actualPayload);
    }
}
