using Microsoft.Extensions.Options;
using SmartHome.TelemetryWorker.Configuration;
using SmartHome.TelemetryWorker.Mqtt.Connection;
using SmartHome.TelemetryWorker.Mqtt.Receiving.Handlers.Telemetry;

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

        services.AddScoped<ClimateMeasurementMessageHandler>();

        return services;
    }
}
