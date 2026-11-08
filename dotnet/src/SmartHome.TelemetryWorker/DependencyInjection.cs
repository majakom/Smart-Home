using Microsoft.Extensions.Options;
using SmartHome.TelemetryWorker.Configuration;
using SmartHome.TelemetryWorker.HostedServices;
using SmartHome.TelemetryWorker.Mqtt.Connection;
using SmartHome.TelemetryWorker.Mqtt.Receiving;
using SmartHome.TelemetryWorker.Mqtt.Receiving.Telemetry;
using SmartHome.TelemetryWorker.Mqtt.Receiving.Telemetry.Handlers;

namespace SmartHome.TelemetryWorker;

public static class DependencyInjection
{
    public static IServiceCollection AddTelemetryWorker(
        this IServiceCollection services,
        IConfiguration configuration
    )
    {
        services.AddSingleton<
            IValidateOptions<MqttOptions>,
            MqttOptionsValidator
        >();

        services
            .AddOptions<MqttOptions>()
            .Bind(configuration.GetSection(MqttOptions.SectionName))
            .ValidateOnStart();

        services.AddSingleton<MqttCertificateLoader>();
        services.AddSingleton<MqttClientFactory>();

        services.AddSingleton<IMqttMessageSubscriber, TelemetrySubscriber>();
        services.AddSingleton<TelemetryMessageDispatcher>();
        services.AddSingleton<MqttMessageProcessor>();

        services.AddSingleton<TelemetryOutcomeLogger>();

        services.AddScoped<ITelemetryMessageHandler, ClimateMeasurementMessageHandler>();

        services.AddHostedService<MqttHostedService>();

        return services;
    }
}
