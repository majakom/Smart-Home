using SmartHome.Application.Abstractions.Sensors;
using SmartHome.Application.UseCases.Telemetry.Shared.Results;
using SmartHome.Domain.Sensors;

namespace SmartHome.Application.UseCases.Telemetry.Shared.Validation;

public sealed class TelemetryQuerySensorValidator
{
    private readonly ISensorReader _sensorReader;

    public TelemetryQuerySensorValidator(ISensorReader sensorReader)
    {
        _sensorReader = sensorReader;
    }

    public async Task<TelemetryQueryFailureReason?> ValidateAsync(
        SensorId sensorId,
        SensorCategory expectedCategory,
        CancellationToken cancellationToken
    )
    {
        // 1. Validate sensorId
        if (sensorId.IsEmpty)
            return TelemetryQueryFailureReason.InvalidSensorId;

        // 2. Retrieve sensor
        Sensor? sensor = await _sensorReader.GetByIdAsync(
            sensorId,
            cancellationToken
        );

        if (sensor is null)
            return TelemetryQueryFailureReason.SensorNotFound;

        // 3. Validate category matching
        if (sensor.Category != expectedCategory)
            return TelemetryQueryFailureReason.InvalidSensorCategory;

        return null;
    }
}
