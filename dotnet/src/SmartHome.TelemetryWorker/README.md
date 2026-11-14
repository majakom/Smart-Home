# SmartHome.TelemetryWorker

Hosts the MQTT telemetry receiver. It manages the broker connection and routes incoming messages to Application use cases.

## Configuration

Required settings:

- `ConnectionStrings__SmartHome` for PostgreSQL.
- `Mqtt__Host`, `Mqtt__Port`, `Mqtt__Credentials__Username`, and `Mqtt__Credentials__Password` for the broker.
- `Mqtt__Tls__CaCertificatePath`, `Mqtt__Tls__ClientCertificatePath`, and `Mqtt__Tls__ClientKeyPath` for TLS.

`appsettings.json` contains non-secret defaults. Supply settings through environment variables or a root `.env` file. Relative certificate paths resolve from the application output directory; deployments must provide the certificate files. Do not commit credentials or private keys.

## Run locally

Prerequisites: .NET 10 SDK, PostgreSQL with migrations applied, a reachable MQTT broker, and the required TLS files. Devices and sensors referenced by incoming topics must already exist in the database.

From the repository root:

```powershell
dotnet run --project .\dotnet\src\SmartHome.TelemetryWorker\SmartHome.TelemetryWorker.csproj
```

## Structure and responsibilities

```text
SmartHome.TelemetryWorker/
├── Configuration/  # MQTT options and validation
├── HostedServices/ # MQTT client lifecycle
├── Mqtt/
│   ├── Connection/ # Client setup and TLS certificates
│   ├── Contracts/  # Incoming payload models
│   ├── Receiving/  # Subscribers, dispatch, and handlers
│   └── Topics/     # Topic filters and parsing
└── certificates/   # Local TLS files
```

The worker receives MQTT messages and routes them to handlers, which map payloads into Application requests. For supported message topics and their handling, see the [MQTT Topics guide](Mqtt/Topics/README.md).
