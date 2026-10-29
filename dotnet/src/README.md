# .NET solution architecture

This directory contains the .NET part of SmartHome. The solution is organized by architectural layer and executable host: core business code is kept separate from database and network adapters, while each runnable process has its own project.

The projects are being scaffolded, so this document describes their intended responsibilities and runtime flow. It is an architecture plan, not a claim that all features or containers are implemented yet.

## Projects and responsibilities

| Project | Responsibility | References |
| --- | --- | --- |
| `SmartHome.Domain` | Business entities, value objects, domain rules, and domain events. It should not depend on other SmartHome projects or infrastructure frameworks. | None |
| `SmartHome.Application` | Use cases, input/output models, application-level validation, and interfaces needed by use cases. Coordinates domain behavior without knowing how persistence or external services work. | `Domain` |
| `SmartHome.Infrastructure` | Adapters for PostgreSQL, InfluxDB, and other external services. Holds EF Core persistence configuration and migrations. Implements interfaces defined by the inner layers. | `Application`, `Domain` |
| `SmartHome.Api` | ASP.NET Core HTTP host. Defines HTTP endpoints, translates requests to application use cases, and configures dependency injection. It is the API consumed by the web and future mobile clients. | `Application`, `Infrastructure` |
| `SmartHome.TelemetryWorker` | Long-running host that receives telemetry from MQTT and sends it through application use cases for validation and storage. | `Application`, `Infrastructure` |
| `SmartHome.DbMigrator` | One-shot executable that connects to PostgreSQL, applies pending EF Core migrations, then exits. | `Infrastructure` |
| `SmartHome.Web` | Blazor WebAssembly user interface. Runs in the browser and calls `SmartHome.Api` over HTTP; it must not connect directly to PostgreSQL or the broker. | No server-side project reference; communicates over HTTP |

The planned MAUI client is deliberately not included yet. When it is added at the end of the project, it will use MVVM on the client and call the same HTTP API as `SmartHome.Web`.

## Dependency direction

Dependencies point inward toward business rules. Infrastructure and hosts may depend on inner layers; Domain and Application do not depend on the API, UI, database, or broker implementation.

```text
Domain
  ↑
Application
  ↑
Infrastructure
  ↑
API ───────────────┐
TelemetryWorker ───┼── runnable server-side hosts
DbMigrator ────────┘

Web (Blazor WebAssembly) ── HTTP ──> API
Future Mobile (MAUI/MVVM) ─ HTTP ──> API
```

`Api`, `TelemetryWorker`, and `DbMigrator` are composition roots: each configures dependency injection and selects the Infrastructure implementations for Application interfaces. They are separate processes with separate jobs, not layers that call each other.

## Main logic flows

### User request

```text
Browser / future MAUI app
  → HTTP request
  → SmartHome.Api endpoint
  → SmartHome.Application use case
  → Domain rules
  → Application persistence interface
  → SmartHome.Infrastructure adapter
  → PostgreSQL
```

The result returns through the same path and is serialized by the API as an HTTP response. The clients only know the API contract, so the backend implementation can evolve without giving either client direct database access.

### Incoming sensor telemetry

```text
Device (station_onboard)
  → MQTT over TLS
  → Mosquitto broker
  → SmartHome.TelemetryWorker
  → SmartHome.Application use case
  → Domain validation/rules
  → SmartHome.Infrastructure persistence adapter
  → PostgreSQL
```

PostgreSQL is the durable intake point for measurements. The worker owns the MQTT subscription/receive loop and passes parsed measurements into Application rather than putting business rules in the MQTT callback or database adapter.

### Long-term measurement history

```text
PostgreSQL (pending/unsynced measurements)
  → synchronization use case / worker
  → SmartHome.Infrastructure InfluxDB adapter
  → InfluxDB Cloud (long-term measurement history)
```

Keep PostgreSQL as the source of truth for operational data such as devices and configuration, and for measurements until they have been successfully synchronized. InfluxDB Cloud is the long-term store for measurement history. A measurement should only be marked synchronized after the write to InfluxDB succeeds, so a failed sync can be retried. Historical queries can then use InfluxDB through an Application use case and its Infrastructure adapter; clients still access data only through the API.

### Database schema changes

The EF Core model and its configuration in `SmartHome.Infrastructure` define the persistence mapping. Generated, committed migrations and the model snapshot belong in that project, for example under `Persistence/Migrations/`. `SmartHome.DbMigrator` runs those migrations against the configured PostgreSQL database and then shuts down. `infrastructure/postgres/` remains for the PostgreSQL runtime configuration and operational documentation; it is not a second copy of the application schema.

For CLI migration commands and local setup, see [Persistence README](SmartHome.Infrastructure/Persistence/README.md). That workflow uses `dotnet ef` directly and does not use `SmartHome.DbMigrator`.

## Planned container setup

The root `docker-compose.yml` currently defines the PostgreSQL and Mosquitto infrastructure services. InfluxDB is planned as InfluxDB Cloud, so it is an external managed service rather than a local Compose container. The following application containers are planned as the .NET implementation grows:

| Container | Runs | Connects to |
| --- | --- | --- |
| `smarthome-postgres` | PostgreSQL with persistent data volume | API, telemetry worker, and migrator over the Compose network |
| `smarthome-mosquitto` | MQTT broker with TLS configuration and persistent broker data/logs | Devices from the host/LAN; telemetry worker over the Compose network |
| `smarthome-db-migrator` | One-shot `SmartHome.DbMigrator`; exits after migrations complete | PostgreSQL |
| `smarthome-api` | ASP.NET Core API | PostgreSQL and any required backend integrations |
| `smarthome-telemetry-worker` | MQTT consumer and measurement synchronization background process | Mosquitto, PostgreSQL, and InfluxDB Cloud through application/infrastructure adapters |
| `smarthome-web` | Built Blazor WebAssembly static assets served over HTTP(S) | Browser users; browser calls the API |
| InfluxDB Cloud (external) | Managed long-term measurement history store; not a Docker Compose container | API and/or telemetry worker through the InfluxDB adapter, as required by use cases |

All server-side containers should use Compose service DNS names (for example `postgres` and `mosquitto`) and container ports, rather than `localhost`. `localhost` inside a container means that same container. Browser code is different: it calls an API URL reachable from the user's browser, configured for the selected deployment environment.

The intended startup relationship is:

```text
PostgreSQL healthy ──> DbMigrator completes ──> API starts
Mosquitto healthy ─────────────────────────────> TelemetryWorker starts
API available ─────────────────────────────────> Web UI can make requests
```

Compose health checks and service dependency conditions can coordinate startup, but the API and worker should still handle temporary connection failures and retry as appropriate. A successful container start alone does not prove that the application can communicate correctly with its dependencies.
