using Microsoft.Extensions.DependencyInjection;
using SmartHome.Application.UseCases.Telemetry.RecordClimateMeasurement;
using SmartHome.Application.UseCases.Telemetry.Shared.Validation;

namespace SmartHome.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(
        this IServiceCollection services
    )
    {
        services.AddSingleton<TimeProvider>(TimeProvider.System);

        services.AddSingleton<TelemetryRequestValidator>();
        services.AddSingleton<RecordClimateMeasurementRequestValidator>();

        services.AddScoped<TelemetrySourceValidator>();
        services.AddScoped<RecordClimateMeasurementUseCase>();

        return services;
    }
}
