namespace SmartHome.Api.Features.Sensors.Measurements.Climate;

public sealed record ClimateMeasurementResponse(
    Guid Id,
    long SourceMessageId,
    Guid SensorId,
    DateTime MeasuredAtUtc,
    DateTime ReceivedAtUtc,
    decimal? TemperatureCelsius,
    decimal? RelativeHumidityPercent
);
