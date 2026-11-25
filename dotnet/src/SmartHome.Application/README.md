# SmartHome.Application

Coordinates application workflows using Domain types and abstractions implemented by outer layers. It has no database, MQTT, or HTTP dependencies and is not a standalone executable.

## Structure and responsibilities

```text
SmartHome.Application/
├── Abstractions/
│   ├── Devices/       # Device lookup and presence-update used by telemetry received
│   ├── Sensors/       # Sensor lookup required by telemetry validation and queries
│   └── Telemetry/     # Measurement read and write operations
├── UseCases/
│   ├── Devices/
│   │   ├── GetDeviceById/ # Retrieve one device
│   │   └── GetDevices/    # Retrieve all devices
│   ├── Sensors/
│   │   ├── GetSensorById/ # Retrieve one sensor
│   │   └── GetSensors/    # Retrieve all sensors
│   └── Telemetry/
│       ├── GetSensorClimateMeasurements/ # Query climate measurements for a sensor
│       ├── RecordClimateMeasurement/     # Record an incoming climate measurement
│       └── Shared/
│           ├── Results/                  # Outcomes and rejection reasons
│           └── Validation/               # Shared telemetry validation
└── DependencyInjection.cs
```

Hosts register use cases and validators with `services.AddApplication()`. Infrastructure registers implementations of the abstractions separately. For the current workflows and their outcomes, see the [UseCases guide](UseCases/README.md).
