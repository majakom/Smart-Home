using SmartHome.Domain.Measurements;

namespace SmartHome.Api.Features.Sensors.Measurements.Climate;

public static class ClimateMeasurementResponseMapper
{
    public static ClimateMeasurementResponse Map(ClimateMeasurement measurement)
    {
        return new ClimateMeasurementResponse(
            measurement.Id.Value,
            measurement.SourceMessageId.Value,
            measurement.SensorId.Value,
            measurement.MeasuredAtUtc,
            measurement.ReceivedAtUtc,
            measurement.TemperatureCelsius,
            measurement.RelativeHumidityPercent
        );
    }
}
