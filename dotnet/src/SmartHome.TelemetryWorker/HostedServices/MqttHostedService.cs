using Microsoft.Extensions.Options;
using MQTTnet;
using SmartHome.TelemetryWorker.Configuration;
using SmartHome.TelemetryWorker.Mqtt.Connection;
using SmartHome.TelemetryWorker.Mqtt.Receiving;
using MqttClientFactory = SmartHome.TelemetryWorker.Mqtt.Connection.MqttClientFactory;

namespace SmartHome.TelemetryWorker.HostedServices;

public sealed class MqttHostedService : BackgroundService
{
    private readonly MqttOptions _options;
    private readonly MqttCertificateLoader _certificateLoader;
    private readonly MqttClientFactory _clientFactory;
    private readonly MqttMessageProcessor _messageProcessor;
    private readonly ILogger<MqttHostedService> _logger;

    public MqttHostedService(
        IOptions<MqttOptions> options,
        MqttCertificateLoader certificateLoader,
        MqttClientFactory clientFactory,
        MqttMessageProcessor messageProcessor,
        ILogger<MqttHostedService> logger
    )
    {
        _options = options.Value;
        _certificateLoader = certificateLoader;
        _clientFactory = clientFactory;
        _messageProcessor = messageProcessor;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using MqttTlsCertificates certificates = _certificateLoader.Load(_options.Tls);

        using IMqttClient client = _clientFactory.CreateClient();

        MqttClientOptions clientOptions = _clientFactory.CreateOptions(_options, certificates);

        _messageProcessor.RegisterHandlers(client, stoppingToken);

        try
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                TaskCompletionSource<bool> disconnectSignal = new(TaskCreationOptions.RunContinuationsAsynchronously);
                Func<MqttClientDisconnectedEventArgs, Task> disconnectedHandler = _ =>
                {
                    disconnectSignal.TrySetResult(true);
                    return Task.CompletedTask;
                };

                client.DisconnectedAsync += disconnectedHandler;

                try
                {
                    _logger.LogInformation(
                        "connecting to Mqtt broker {Host}:{Port}.",
                        _options.Host,
                        _options.Port
                    );

                    await client.ConnectAsync(clientOptions, stoppingToken);

                    await _messageProcessor.SubscribeAsync(client, stoppingToken);

                    _logger.LogInformation("Connected and subscribed to MQTT.");

                    await disconnectSignal.Task.WaitAsync(stoppingToken);

                    if (!stoppingToken.IsCancellationRequested)
                        _logger.LogWarning("MQTT connection was lost.");
                }
                catch (OperationCanceledException)
                    when (stoppingToken.IsCancellationRequested)
                {
                    break;
                }
                catch (Exception exception)
                {
                    _logger.LogError(
                        exception,
                        "MQTT connection or subscription failed."
                    );
                }
                finally
                {
                    client.DisconnectedAsync -= disconnectedHandler;
                }

                if (stoppingToken.IsCancellationRequested)
                    break;
                
                if (client.IsConnected)
                {
                    try
                    {
                        await client.DisconnectAsync(cancellationToken: CancellationToken.None);
                    }
                    catch (Exception exception)
                    {
                        _logger.LogWarning(
                            exception,
                            "Failed to disconnect MQTT client before reconnecting."
                        );
                    }
                }

                _logger.LogInformation(
                    "Reconnecting to MQTT broker in {DelaySeconds} seconds.",
                    _options.ReconnectDelaySeconds
                );

                await Task.Delay(TimeSpan.FromSeconds(_options.ReconnectDelaySeconds), stoppingToken);
            }
        }
        finally
        {
            if (client.IsConnected)
            {
                try
                {
                    await client.DisconnectAsync(cancellationToken: CancellationToken.None);
                }
                catch (Exception exception)
                {
                    _logger.LogWarning(
                        exception,
                        "Failed to disconnect MQTT client during shutdown."
                    );
                }
            }
        }
    }
}
