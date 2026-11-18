using SmartHome.Domain.Sensors;

namespace SmartHome.Application.Abstractions.Sensors;

public interface ISensorReader
{
    Task<Sensor?> GetByIdAsync(
        SensorId sensorId,
        CancellationToken cancellationToken
    );
}
