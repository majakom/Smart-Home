using Microsoft.Extensions.Options;
using SmartHome.TelemetryWorker.Configuration;

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

        return services;
    }
}
