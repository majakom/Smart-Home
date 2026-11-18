using SmartHome.Domain.Measurements;
using SmartHome.Domain.Sensors;

namespace SmartHome.Application.Abstractions.Telemetry;

public interface IClimateMeasurementReader
{
    Task<IReadOnlyList<ClimateMeasurement>> GetBySensorIdAsync(
        SensorId sensorId,
        DateTime fromUtc,
        DateTime toUtc,
        CancellationToken cancellationToken
    );
}
