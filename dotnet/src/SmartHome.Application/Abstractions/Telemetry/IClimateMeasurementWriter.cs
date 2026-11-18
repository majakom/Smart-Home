using SmartHome.Domain.Measurements;

namespace SmartHome.Application.Abstractions.Telemetry;

public interface IClimateMeasurementWriter
{
    Task<bool> TryAddAsync(
        ClimateMeasurement measurement,
        CancellationToken cancellationToken
    );
}
