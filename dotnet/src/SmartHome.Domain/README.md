# SmartHome.Domain

Contains business entities, value objects, and rules. It has no project or third-party package dependencies and is not a standalone executable.

## Structure and responsibilities

```text
SmartHome.Domain/
├── Devices/       # Device identity and enabled state
├── Sensors/       # Sensor category, identity, and device association
├── Measurements/  # Climate readings and source message identifiers
└── Shared/        # Value objects shared across domain concepts
```

Application uses these types to implement workflows; Infrastructure maps them to persistence. Entities represent mutable business concepts such as devices and sensors, while value objects represent validated identifiers and other domain values. Constructors and domain operations enforce invariants.

The project is independent of MQTT payloads, HTTP contracts, and database mapping.
