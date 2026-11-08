# MQTT Telemetry Topics

Each telemetry category has a topic format and maps to a Domain `SensorCategory`. The worker parses the device and sensor IDs from the topic, then routes the message to the corresponding handler.

## Shared format

```text
smarthome/devices/{deviceId}/sensors/{sensorId}/telemetry/{category}
```

IDs must be non-empty GUIDs in `D` format. Category names are explicit MQTT wire names, not values derived from Domain enum names.

## Supported categories

### Climate

**Topic:**

```text
smarthome/devices/{deviceId}/sensors/{sensorId}/telemetry/climate
```

`climate` maps to `SensorCategory.Climate`. The worker deserializes the payload and passes it with the parsed device and sensor IDs to the Application climate-measurement use case.

When another category is implemented, add a matching subsection here with its topic, Domain category, and handler behavior. Update `MqttTelemetryCategories` and the topic filters/parser to support its explicit wire name.
