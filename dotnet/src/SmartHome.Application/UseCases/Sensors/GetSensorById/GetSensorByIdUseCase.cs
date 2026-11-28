using SmartHome.Application.Abstractions.Sensors;
using SmartHome.Domain.Sensors;

namespace SmartHome.Application.UseCases.Sensors.GetSensorById;

public sealed class GetSensorByIdUseCase
{
    private readonly ISensorReader _sensorReader;

    public GetSensorByIdUseCase(ISensorReader sensorReader)
    {
        _sensorReader = sensorReader;
    }

    public Task<Sensor?> ExecuteAsync(
        SensorId sensorId,
        CancellationToken cancellationToken = default
    )
    {
        return _sensorReader.GetByIdAsync(sensorId, cancellationToken);
    }
}
