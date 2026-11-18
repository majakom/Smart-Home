using System.Buffers;
using SmartHome.TelemetryWorker.Mqtt.Receiving.Telemetry.Handlers;
using SmartHome.TelemetryWorker.Mqtt.Topics.Telemetry;

namespace SmartHome.TelemetryWorker.Mqtt.Receiving.Telemetry;

public sealed class TelemetryMessageDispatcher
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<TelemetryMessageDispatcher> _logger;

    public TelemetryMessageDispatcher(
        IServiceScopeFactory scopeFactory,
        ILogger<TelemetryMessageDispatcher> logger
    )
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    public async Task DispatchAsync(
        string topic,
        ReadOnlyMemory<byte> payload,
        CancellationToken cancellationToken
    )
    {
        if (!TelemetryTopicParser.TryParse(
            topic,
            out TelemetryTopic? telemetryTopic,
            out TelemetryTopicParseFailure failure
        ))
        {
            _logger.LogWarning(
                "Ignoring MQTT message with invalid telemetry topic {Topic}. Reason: {Reason}",
                topic,
                failure
            );
            return;
        }

        await using AsyncServiceScope scope = _scopeFactory.CreateAsyncScope();

        ITelemetryMessageHandler[] matchingHandlers = scope.ServiceProvider
            .GetServices<ITelemetryMessageHandler>()
            .Where(handler => handler.Category == telemetryTopic.Category)
            .Take(2)
            .ToArray();

        if (matchingHandlers.Length == 0)
        {
            _logger.LogWarning(
                "No handler is registered for telemetry category {Category}.",
                telemetryTopic.Category);
            return;
        }

        if (matchingHandlers.Length > 1)
        {
            throw new InvalidOperationException(
                $"More than one telemetry handler is registered for category '{telemetryTopic.Category}'.");
        }

        ITelemetryMessageHandler handler = matchingHandlers[0];

        await handler.HandleAsync(
            telemetryTopic,
            payload,
            cancellationToken
        );
    }
}
