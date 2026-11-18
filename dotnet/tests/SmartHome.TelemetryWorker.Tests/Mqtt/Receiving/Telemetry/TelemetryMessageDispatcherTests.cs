using System.Text;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NSubstitute;
using SmartHome.Domain.Devices;
using SmartHome.Domain.Sensors;
using SmartHome.TelemetryWorker.Mqtt.Receiving.Telemetry;
using SmartHome.TelemetryWorker.Mqtt.Receiving.Telemetry.Handlers;
using SmartHome.TelemetryWorker.Mqtt.Topics.Telemetry;

namespace SmartHome.TelemetryWorker.Tests.Mqtt.Receiving.Telemetry;

public class TelemetryMessageDispatcherTests
{
    [Fact]
    public async Task DispatchAsync_InvokesMatchingHandlerWithParsedTopicAndPayload()
    {
        // Arrange
        Guid deviceGuid = Guid.NewGuid();
        Guid sensorGuid = Guid.NewGuid();

        string topic =
            $"smarthome/devices/{deviceGuid:D}/sensors/{sensorGuid:D}/telemetry/climate";

        TelemetryTopic expectedTopic = new(
            new DeviceId(deviceGuid),
            new SensorId(sensorGuid),
            SensorCategory.Climate
        );

        ReadOnlyMemory<byte> payload = Encoding.UTF8.GetBytes(
            """{"sourceMessageId":42,"temperatureCelsius":21.5}"""
        );

        ITelemetryMessageHandler handler = Substitute.For<ITelemetryMessageHandler>();
        handler.Category.Returns(SensorCategory.Climate);

        ITelemetryMessageHandler otherHandler = Substitute.For<ITelemetryMessageHandler>();
        otherHandler.Category.Returns((SensorCategory)999);

        using ServiceProvider serviceProvider = CreateServiceProvider(handler, otherHandler);

        RecordingLogger<TelemetryMessageDispatcher> logger = new();

        TelemetryMessageDispatcher dispatcher = new(
            serviceProvider.GetRequiredService<IServiceScopeFactory>(),
            logger
        );

        using CancellationTokenSource cancellationSource = new();

        // Act
        await dispatcher.DispatchAsync(
            topic,
            payload,
            cancellationSource.Token
        );

        // Assert
        await handler.Received(1).HandleAsync(
            expectedTopic,
            payload,
            cancellationSource.Token
        );

        await otherHandler.DidNotReceive().HandleAsync(
            Arg.Any<TelemetryTopic>(),
            Arg.Any<ReadOnlyMemory<byte>>(),
            Arg.Any<CancellationToken>()
        );

        Assert.Empty(logger.Entries);
    }

    [Fact]
    public async Task DispatchAsync_DoesNotInvokeHandler_WhenTopicIsInvalid()
    {
        // Arrange
        ReadOnlyMemory<byte> payload = Encoding.UTF8.GetBytes("{}");

        ITelemetryMessageHandler handler = Substitute.For<ITelemetryMessageHandler>();
        handler.Category.Returns(SensorCategory.Climate);

        using ServiceProvider serviceProvider = CreateServiceProvider(handler);

        RecordingLogger<TelemetryMessageDispatcher> logger = new();

        TelemetryMessageDispatcher dispatcher = new(
            serviceProvider.GetRequiredService<IServiceScopeFactory>(),
            logger
        );

        // Act
        await dispatcher.DispatchAsync(
            "invalid/topic",
            payload,
            CancellationToken.None
        );

        // Assert
        await handler.DidNotReceive().HandleAsync(
            Arg.Any<TelemetryTopic>(),
            Arg.Any<ReadOnlyMemory<byte>>(),
            Arg.Any<CancellationToken>()
        );

        RecordedLog warningLog = Assert.Single(logger.Entries);
        Assert.Equal(LogLevel.Warning, warningLog.Level);
        Assert.Contains("invalid telemetry topic", warningLog.Message);
        Assert.Contains("invalid/topic", warningLog.Message);
    }

    [Fact]
    public async Task DispatchAsync_DoesNotInvokeHandler_WhenNoHandlerMatchesCategory()
    {
        // Arrange
        string topic = CreateValidClimateTopic();
        ReadOnlyMemory<byte> payload = Encoding.UTF8.GetBytes("{}");

        ITelemetryMessageHandler handler = Substitute.For<ITelemetryMessageHandler>();
        handler.Category.Returns((SensorCategory)999);

        using ServiceProvider serviceProvider = CreateServiceProvider(handler);

        RecordingLogger<TelemetryMessageDispatcher> logger = new();

        TelemetryMessageDispatcher dispatcher = new(
            serviceProvider.GetRequiredService<IServiceScopeFactory>(),
            logger
        );

        // Act
        await dispatcher.DispatchAsync(
            topic,
            payload,
            CancellationToken.None
        );

        // Assert
        await handler.DidNotReceive().HandleAsync(
            Arg.Any<TelemetryTopic>(),
            Arg.Any<ReadOnlyMemory<byte>>(),
            Arg.Any<CancellationToken>()
        );

        RecordedLog warningLog = Assert.Single(logger.Entries);
        Assert.Equal(LogLevel.Warning, warningLog.Level);
        Assert.Contains("No handler is registered", warningLog.Message);
        Assert.Contains("Climate", warningLog.Message);
    }

    [Fact]
    public async Task DispatchAsync_ThrowsInvalidOperationException_WhenMultipleHandlersMatchCategory()
    {
        // Arrange
        string topic = CreateValidClimateTopic();
        ReadOnlyMemory<byte> payload = Encoding.UTF8.GetBytes("{}");

        ITelemetryMessageHandler firstHandler = Substitute.For<ITelemetryMessageHandler>();
        ITelemetryMessageHandler secondHandler = Substitute.For<ITelemetryMessageHandler>();
        firstHandler.Category.Returns(SensorCategory.Climate);
        secondHandler.Category.Returns(SensorCategory.Climate);

        using ServiceProvider serviceProvider = CreateServiceProvider(firstHandler, secondHandler);

        RecordingLogger<TelemetryMessageDispatcher> logger = new();

        TelemetryMessageDispatcher dispatcher = new(
            serviceProvider.GetRequiredService<IServiceScopeFactory>(),
            logger
        );

        // Act
        Func<Task> act = () => dispatcher.DispatchAsync(topic, payload, CancellationToken.None);

        // Assert
        InvalidOperationException exception = await Assert.ThrowsAsync<InvalidOperationException>(act);
        Assert.Contains("More than one telemetry handler is registered", exception.Message);

        await firstHandler.DidNotReceive().HandleAsync(
            Arg.Any<TelemetryTopic>(),
            Arg.Any<ReadOnlyMemory<byte>>(),
            Arg.Any<CancellationToken>()
        );

        await secondHandler.DidNotReceive().HandleAsync(
            Arg.Any<TelemetryTopic>(),
            Arg.Any<ReadOnlyMemory<byte>>(),
            Arg.Any<CancellationToken>()
        );
    }

    private static string CreateValidClimateTopic()
    {
        Guid deviceId = Guid.NewGuid();
        Guid sensorId = Guid.NewGuid();

        return
            $"smarthome/devices/{deviceId:D}/sensors/{sensorId:D}/telemetry/climate";
    }

    private static ServiceProvider CreateServiceProvider(
        params ITelemetryMessageHandler[] handlers)
    {
        ServiceCollection services = new();

        foreach (ITelemetryMessageHandler handler in handlers)
        {
            services.AddSingleton(handler);
        }

        return services.BuildServiceProvider();
    }

    private sealed class RecordingLogger<T> : ILogger<T>
    {
        public List<RecordedLog> Entries { get; } = [];

        public IDisposable? BeginScope<TState>(TState state)
            where TState : notnull
        {
            return null;
        }

        public bool IsEnabled(LogLevel logLevel) => true;

        public void Log<TState>(
            LogLevel logLevel,
            EventId eventId,
            TState state,
            Exception? exception,
            Func<TState, Exception?, string> formatter)
        {
            Entries.Add(
                new RecordedLog(
                    logLevel,
                    formatter(state, exception)
                )
            );
        }
    }

    private sealed record RecordedLog(
        LogLevel Level,
        string Message
    );
}
