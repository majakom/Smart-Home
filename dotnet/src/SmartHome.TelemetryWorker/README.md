# SmartHome.TelemetryWorker

Hosts the MQTT telemetry receiver. It manages the broker connection and routes incoming messages to Application use cases.

## Configuration

Required settings:

- `ConnectionStrings__SmartHome` for PostgreSQL.
- `Mqtt__Host`, `Mqtt__Port`, `Mqtt__Credentials__Username`, and `Mqtt__Credentials__Password` for the broker.
- `Mqtt__Tls__CaCertificatePath`, `Mqtt__Tls__ClientCertificatePath`, and `Mqtt__Tls__ClientKeyPath` for TLS.

`appsettings.json` contains non-secret defaults. Supply settings through environment variables or a root `.env` file. Relative certificate paths resolve from the application output directory.

**Minimal requirements** for local and docker development with the default MQTT settings:

- Set `Mqtt__Credentials__Password` in the root `.env` file; the default `Mqtt__Credentials__Username` is `dotnet-client`.
- Follow the [Mosquitto guide](../../../infrastructure/mosquitto/README.md) to configure the broker credentials and generate the certificates for `dotnet-client`.
- Add generated `ca.crt`, `client.crt`, and `client.key` to `dotnet/src/SmartHome.TelemetryWorker/certificates/`.

## Run locally

Prerequisites: .NET 10 SDK, PostgreSQL with migrations applied, a reachable MQTT broker, and the required TLS files. Devices and sensors referenced by incoming topics must already exist in the database.

From the repository root:

```powershell
dotnet run --project .\dotnet\src\SmartHome.TelemetryWorker\SmartHome.TelemetryWorker.csproj
```

## Run in a container

Run the Compose service:

```powershell
docker compose up --build -d telemetry-worker
```

This service depends on `postgres`, `mosquitto` and `db-migrator` containers, so the command above starts all of them automatically.
Compose mounts the Worker certificate directory read-only at `/app/certificates/`.

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
