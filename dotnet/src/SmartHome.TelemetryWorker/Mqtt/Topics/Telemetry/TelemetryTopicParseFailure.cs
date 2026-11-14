namespace SmartHome.TelemetryWorker.Mqtt.Topics.Telemetry;

public enum TelemetryTopicParseFailure
{
    None,
    EmptyTopic,
    InvalidShape,
    InvalidDeviceId,
    InvalidSensorId,
    UnsupportedCategory
}
