using SmartHome.Domain.Sensors;

namespace SmartHome.Application.UseCases.Telemetry.GetSensorClimateMeasurements;

public sealed record GetSensorClimateMeasurementsRequest(
    SensorId SensorId,
    DateTime FromUtc,
    DateTime ToUtc
);
