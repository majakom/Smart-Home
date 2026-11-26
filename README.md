# Smart-Home

Local services are defined in the root `docker-compose.yml`. Run Docker Compose commands from the repository root.

## Configuration guides

- [PostgreSQL](infrastructure/postgres/README.md) — environment variables, startup, and persistent storage.
- [Mosquitto](infrastructure/mosquitto/README.md) — TLS certificates, broker credentials, and client connection tests.
- [Telemetry Worker](dotnet/src/SmartHome.TelemetryWorker/README.md) — local and Docker operation.
- [SmartHome API](dotnet/src/SmartHome.Api/README.md) — local startup, HTTP endpoints, and health checks.

Before starting the stack, copy `.env.example` to `.env` in the repository root and set `POSTGRES_PASSWORD` and `Mqtt__Credentials__Password`. Configure Mosquitto certificates and credentials by following its guide. The Telemetry Worker requires its own `ca.crt`, `client.crt`, and `client.key` under `dotnet/src/SmartHome.TelemetryWorker/certificates/`.

## Run the stack

After configuration, run Docker Compose commands from the repository root.

Build images and start all services in the background:

```powershell
docker compose up --build -d
```

Follow service logs:

```powershell
docker compose logs -f
```

Stop the services while preserving named-volume data:

```powershell
docker compose down
```

## Run .NET tests

Prerequisites: .NET 10 SDK and Docker running. Infrastructure tests use Testcontainers to start PostgreSQL and Mosquitto containers, so Docker must be available even when you are not running the full Compose stack.

From the repository root, run all tests in the solution:

```powershell
dotnet test .\dotnet\SmartHome.slnx
```

To run a single test project, for example:

```powershell
dotnet test .\dotnet\tests\SmartHome.Domain.Tests\
```
