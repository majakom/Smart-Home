using SmartHome.Domain.Devices;
using SmartHome.Domain.Sensors;

namespace SmartHome.TelemetryWorker.Mqtt.Topics.Telemetry;

public sealed record TelemetryTopic(
    DeviceId DeviceId,
    SensorId SensorId,
    SensorCategory Category
);
