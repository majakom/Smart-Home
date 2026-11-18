using SmartHome.Domain.Sensors;

namespace SmartHome.TelemetryWorker.Mqtt.Topics.Telemetry;

public static class MqttTelemetryCategories
{
    public static bool TryParse(
        string value,
        out SensorCategory category
    )
    {
        switch (value)
        {
            case "climate":
                category = SensorCategory.Climate;
                return true;

            default:
                category = default;
                return false;
        }
    }

    public static string GetName(SensorCategory category) =>
        category switch
        {
            SensorCategory.Climate => "climate",
            _ => throw new ArgumentOutOfRangeException(
                nameof(category),
                category,
                "Unsupported MQTT telemetry category."
            )
        };
}
