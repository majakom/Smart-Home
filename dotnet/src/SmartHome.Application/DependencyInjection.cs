using Microsoft.Extensions.DependencyInjection;
using SmartHome.Application.UseCases.Devices.GetDeviceById;
using SmartHome.Application.UseCases.Devices.GetDevices;
using SmartHome.Application.UseCases.Sensors.GetSensors;
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

        services.AddScoped<GetDeviceByIdUseCase>();
        services.AddScoped<GetDevicesUseCase>();

        services.AddScoped<GetSensorsUseCase>();

        services.AddScoped<TelemetrySourceValidator>();
        services.AddScoped<RecordClimateMeasurementUseCase>();

        return services;
    }
}
