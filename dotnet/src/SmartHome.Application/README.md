# SmartHome.Application

Coordinates application workflows using Domain types and abstractions implemented by outer layers. It has no database, MQTT, or HTTP dependencies and is not a standalone executable.

## Structure and responsibilities

```text
SmartHome.Application/
├── Abstractions/
│   ├── Devices/       # Device lookup required by telemetry validation
│   ├── Sensors/       # Sensor lookup required by telemetry validation
│   └── Telemetry/     # Measurement persistence operation
├── UseCases/
│   └── Telemetry/
│       ├── RecordClimateMeasurement/ # Climate measurement workflow
│       └── Shared/
│           ├── Results/              # Outcomes and rejection reasons
│           └── Validation/           # Shared telemetry validation
└── DependencyInjection.cs
```

Hosts register use cases and validators with `services.AddApplication()`. Infrastructure registers implementations of the abstractions separately. For the current workflow and its outcomes, see the [UseCases guide](UseCases/README.md).
