using System.Text.Json.Serialization;

namespace SmartHome.TelemetryWorker.Mqtt.Contracts.Incoming.Telemetry;

public sealed class ClimateMeasurementMessage
{
    [JsonPropertyName("sourceMessageId")]
    public required long SourceMessageId { get; init; }

    [JsonPropertyName("measuredAtUtc")]
    public required DateTimeOffset MeasuredAtUtc { get; init; }

    [JsonPropertyName("temperatureCelsius")]
    public required decimal? TemperatureCelsius { get; init; }

    [JsonPropertyName("relativeHumidityPercent")]
    public required decimal? RelativeHumidityPercent { get; init; }
}
