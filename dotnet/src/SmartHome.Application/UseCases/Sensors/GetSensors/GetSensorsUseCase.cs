using SmartHome.Application.Abstractions.Sensors;
using SmartHome.Domain.Sensors;

namespace SmartHome.Application.UseCases.Sensors.GetSensors;

public sealed class GetSensorsUseCase
{
    private readonly ISensorReader _sensorReader;

    public GetSensorsUseCase(ISensorReader sensorReader)
    {
        _sensorReader = sensorReader;
    }

    public async Task<IReadOnlyList<Sensor>> ExecuteAsync(
        CancellationToken cancellationToken = default
    )
    {
        return await _sensorReader.GetAllAsync(cancellationToken);
    }
}
