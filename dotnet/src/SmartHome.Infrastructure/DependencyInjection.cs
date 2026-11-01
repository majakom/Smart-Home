using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SmartHome.Application.Abstractions.Devices;
using SmartHome.Application.Abstractions.Sensors;
using SmartHome.Application.Abstractions.Telemetry;
using SmartHome.Infrastructure.Persistence;
using SmartHome.Infrastructure.Persistence.Adapters.Devices;
using SmartHome.Infrastructure.Persistence.Adapters.Sensors;
using SmartHome.Infrastructure.Persistence.Adapters.Telemetry;

namespace SmartHome.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        string connectionString)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(connectionString);

        services.AddDbContext<SmartHomeDbContext>(options =>
            options.UseNpgsql(connectionString));

        services.AddScoped<IDeviceReader, EfDeviceReader>();
        services.AddScoped<ISensorReader, EfSensorReader>();
        services.AddScoped<IClimateMeasurementWriter, EfClimateMeasurementWriter>();

        return services;
    }
}
