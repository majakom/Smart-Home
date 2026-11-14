using System.Diagnostics.CodeAnalysis;
using SmartHome.Domain.Devices;
using SmartHome.Domain.Sensors;
using SmartHome.TelemetryWorker.Mqtt.Topics.Shared;

namespace SmartHome.TelemetryWorker.Mqtt.Topics.Telemetry;

/// <summary>
/// Parses MQTT telemetry topics using the following convention:
/// smarthome/devices/{deviceId}/sensors/{sensorId}/telemetry/{category}.
///
/// Device and sensor identifiers are extracted from the topic.
/// The category is mapped to a corresponding SensorCategory.
/// </summary>
public static class TelemetryTopicParser
{
    public static bool TryParse(
        string? topic,
        [NotNullWhen(true)] out TelemetryTopic? telemetryTopic,
        out TelemetryTopicParseFailure failure
    )
    {
        telemetryTopic = null;
        failure = TelemetryTopicParseFailure.None;

        if (string.IsNullOrWhiteSpace(topic))
        {
            failure = TelemetryTopicParseFailure.EmptyTopic;
            return false;
        }

        string[] segments = topic.Split('/');

        if (segments.Length != 7
            || segments[0] != MqttTopicSegments.Root
            || segments[1] != MqttTopicSegments.Devices
            || segments[3] != MqttTopicSegments.Sensors
            || segments[5] != MqttTopicSegments.Telemetry)
        {
            failure = TelemetryTopicParseFailure.InvalidShape;
            return false;
        }

        if (!Guid.TryParseExact(segments[2], "D", out Guid deviceGuid)
            || deviceGuid == Guid.Empty)
        {
            failure = TelemetryTopicParseFailure.InvalidDeviceId;
            return false;
        }

        if (!Guid.TryParseExact(segments[4], "D", out Guid sensorGuid)
            || sensorGuid == Guid.Empty)
        {
            failure = TelemetryTopicParseFailure.InvalidSensorId;
            return false;
        }

        if (!MqttTelemetryCategories.TryParse(segments[6], out SensorCategory category))
        {
            failure = TelemetryTopicParseFailure.UnsupportedCategory;
            return false;
        }

        telemetryTopic = new TelemetryTopic(
            new DeviceId(deviceGuid),
            new SensorId(sensorGuid),
            category
        );

        return true;
    }
}
