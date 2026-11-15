using MQTTnet;
using NSubstitute;
using SmartHome.TelemetryWorker.Mqtt.Receiving;

namespace SmartHome.TelemetryWorker.Tests.Mqtt.Receiving;

public class MqttMessageProcessorTests
{
    [Fact]
    public void RegisterHandlers_ForwardsClientAndCancellationTokenToEverySubscriber()
    {
        // Arrange
        IMqttMessageSubscriber firstSubscriber = Substitute.For<IMqttMessageSubscriber>();
        IMqttMessageSubscriber secondSubscriber = Substitute.For<IMqttMessageSubscriber>();

        IMqttClient mqttClient = Substitute.For<IMqttClient>();
        using CancellationTokenSource cancellationSource = new();

        MqttMessageProcessor processor = new([firstSubscriber, secondSubscriber]);

        // Act
        processor.RegisterHandlers(mqttClient, cancellationSource.Token);

        // Assert
        firstSubscriber.Received(1).RegisterMessageHandler(
            mqttClient,
            cancellationSource.Token
        );

        secondSubscriber.Received(1).RegisterMessageHandler(
            mqttClient,
            cancellationSource.Token
        );
    }

    [Fact]
    public async Task SubscribeAsync_AwaitsEachSubscriberAndForwardsClientAndToken()
    {
        // Arrange
        IMqttMessageSubscriber firstSubscriber = Substitute.For<IMqttMessageSubscriber>();
        IMqttMessageSubscriber secondSubscriber = Substitute.For<IMqttMessageSubscriber>();

        IMqttClient mqttClient = Substitute.For<IMqttClient>();
        using CancellationTokenSource cancellationSource = new();

        TaskCompletionSource<bool> firstSubscription = new(
            TaskCreationOptions.RunContinuationsAsynchronously
        );

        firstSubscriber
            .SubscribeAsync(mqttClient, cancellationSource.Token)
            .Returns(firstSubscription.Task
        );

        secondSubscriber
            .SubscribeAsync(mqttClient, cancellationSource.Token)
            .Returns(Task.CompletedTask
        );

        MqttMessageProcessor processor = new([firstSubscriber, secondSubscriber]);

        // Act
        Task subscribeTask = processor.SubscribeAsync(
            mqttClient,
            cancellationSource.Token
        );

        // Assert
        await firstSubscriber.Received(1).SubscribeAsync(
            mqttClient,
            cancellationSource.Token
        );

        await secondSubscriber.DidNotReceive().SubscribeAsync(
            mqttClient,
            cancellationSource.Token
        );

        firstSubscription.SetResult(true);
        await subscribeTask;

        await secondSubscriber.Received(1).SubscribeAsync(
            mqttClient,
            cancellationSource.Token
        );
    }

    [Fact]
    public void RegisterHandlers_ThrowsArgumentNullException_WhenClientIsNull()
    {
        // Arrange
        IMqttMessageSubscriber subscriber = Substitute.For<IMqttMessageSubscriber>();

        MqttMessageProcessor processor = new([subscriber]);

        // Act
        Action act = () => processor.RegisterHandlers(null!, CancellationToken.None);

        // Assert
        ArgumentNullException exception = Assert.Throws<ArgumentNullException>(act);
        Assert.Equal("mqttClient", exception.ParamName);

        subscriber.DidNotReceive().RegisterMessageHandler(
            Arg.Any<IMqttClient>(),
            Arg.Any<CancellationToken>()
        );
    }

    [Fact]
    public async Task SubscribeAsync_ThrowsArgumentNullException_WhenClientIsNull()
    {
        // Arrange
        IMqttMessageSubscriber subscriber = Substitute.For<IMqttMessageSubscriber>();

        MqttMessageProcessor processor = new([subscriber]);

        // Act
        Func<Task> act = () => processor.SubscribeAsync(null!, CancellationToken.None);

        // Assert
        ArgumentNullException exception = await Assert.ThrowsAsync<ArgumentNullException>(act);
        Assert.Equal("client", exception.ParamName);

        await subscriber.DidNotReceive().SubscribeAsync(
            Arg.Any<IMqttClient>(),
            Arg.Any<CancellationToken>()
        );
    }
}
