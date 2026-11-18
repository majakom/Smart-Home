using SmartHome.Domain.Devices;
using SmartHome.Domain.Measurements;
using SmartHome.Domain.Sensors;

namespace SmartHome.Application.UseCases.Telemetry.RecordClimateMeasurement;

public sealed record RecordClimateMeasurementRequest(
    SourceMessageId SourceMessageId,
    SensorId SensorId,
    DeviceId DeviceId,
    DateTime MeasuredAtUtc,
    decimal? TemperatureCelsius,
    decimal? RelativeHumidityPercent
);
