using SmartHome.Domain.Sensors;
using SmartHome.TelemetryWorker.Mqtt.Topics.Shared;

namespace SmartHome.TelemetryWorker.Mqtt.Topics.Telemetry;

public static class TelemetryTopicFilters
{
    private const string SensorTelemetryPrefix =
        $"{MqttTopicSegments.Root}/" +
        $"{MqttTopicSegments.Devices}/+/" +
        $"{MqttTopicSegments.Sensors}/+/" +
        $"{MqttTopicSegments.Telemetry}/";
    
    public const string All = SensorTelemetryPrefix + "+";
    public static string ForCategory(SensorCategory category)
    {
        return SensorTelemetryPrefix + MqttTelemetryCategories.GetName(category);
    }
}
