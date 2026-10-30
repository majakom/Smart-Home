using SmartHome.Domain.Sensors;

namespace SmartHome.Domain.Measurements;

public sealed class ClimateMeasurement
{
    public ClimateMeasurementId Id { get; private set; }
    public SourceMessageId SourceMessageId { get; private set; }
    public SensorId SensorId { get; private set; }
    public DateTime MeasuredAtUtc { get; private set; }
    public DateTime ReceivedAtUtc { get; private set; }
    public decimal? TemperatureCelsius { get; private set; }
    public decimal? RelativeHumidityPercent { get; private set; }

    private ClimateMeasurement() { }

    public ClimateMeasurement(
        SourceMessageId sourceMessageId,
        SensorId sensorId,
        DateTime measuredAtUtc,
        DateTime receivedAtUtc,
        decimal? temperatureCelsius,
        decimal? relativeHumidityPercent)
    {
        if (sourceMessageId.Value <= 0)
            throw new ArgumentOutOfRangeException(nameof(sourceMessageId), "Source message ID must be a positive sensor sequence number.");

        if (sensorId.IsEmpty)
            throw new ArgumentException("Sensor ID cannot be empty.", nameof(sensorId));

        if (measuredAtUtc.Kind != DateTimeKind.Utc)
            throw new ArgumentException("Measurement timestamp must be in UTC.", nameof(measuredAtUtc));

        if (receivedAtUtc.Kind != DateTimeKind.Utc)
            throw new ArgumentException("Received timestamp must be in UTC.", nameof(receivedAtUtc));

        if (relativeHumidityPercent is < 0m or > 100m)
            throw new ArgumentOutOfRangeException(nameof(relativeHumidityPercent), "Relative humidity must be between 0 and 100 %.");

        Id = ClimateMeasurementId.New();
        SourceMessageId = sourceMessageId;
        SensorId = sensorId;
        MeasuredAtUtc = measuredAtUtc;
        ReceivedAtUtc = receivedAtUtc;
        TemperatureCelsius = temperatureCelsius;
        RelativeHumidityPercent = relativeHumidityPercent;
    }
}
