# SmartHome.API

The API exposes HTTP operations for devices, sensors, and measurements. It calls Application use cases and does not access PostgreSQL directly.

## Configuration

From the repository root, copy `.env.example` to `.env` and set `POSTGRES_PASSWORD` to a local password; it is required and has no default. The other PostgreSQL settings already have defaults.

The example also defines `ConnectionStrings__SmartHome` using those values, with `Host=localhost` for applications running directly on your computer. Keep this connection string in sync if you change the PostgreSQL settings. When run locally, the API loads the root `.env` file. Docker Compose reads the same file and passes the settings into the container.

Start PostgreSQL and apply migrations before running the API. See the root [README](../../../README.md), [PostgreSQL guide](../../../infrastructure/postgres/README.md), and [DbMigrator guide](../SmartHome.DbMigrator/README.md).

## Run locally

Prerequisites: .NET 10 SDK, Docker, a configured root `.env` file and PostgreSQL with migrations applied.

From the repository root:

```powershell
dotnet run --project .\dotnet\src\SmartHome.Api\SmartHome.Api.csproj
```

The default `https` launch profile listens at `https://localhost:5001`. The `http` profile listens at `http://localhost:5000` and can be selected with `--launch-profile http` if needed.

## Run in a container

Prerequisites: Docker and a configured root `.env` file. From the repository root, run:

```powershell
docker compose up --build -d api
```

This service depends on `postgres` and `db-migrator` containers, so the command above starts all of them automatically.

The API is available at `http://localhost:8080` by default. Set `API_PORT` in the root `.env` to change the host port. The `.env.example` sets `ASPNETCORE_ENVIRONMENT=Development`, so Scalar and OpenAPI are available when using that configuration. If the setting is omitted, Compose defaults to `Production`, where they are not mapped.

## Explore the API with Scalar

When the API runs in Development, use Scalar to browse operations, inspect parameters and response codes, and send requests from the browser.

| Run mode | Scalar UI | OpenAPI document |
| --- | --- | --- |
| Local `dotnet run` (`https` profile) | [`https://localhost:5001/scalar`](https://localhost:5001/scalar) | [`https://localhost:5001/openapi/v1.json`](https://localhost:5001/openapi/v1.json) |
| Docker Compose | [`http://localhost:8080/scalar`](http://localhost:8080/scalar) | [`http://localhost:8080/openapi/v1.json`](http://localhost:8080/openapi/v1.json) |

Use [SmartHome.Api.http](SmartHome.Api.http) for the same requests directly from your editor. For Docker Compose, set `@SmartHomeApi_HostAddress = http://localhost:8080` in that file.

## HTTP endpoints

| Method | Route | Description |
| --- | --- | --- |
| `GET` | `/api/devices` | List registered devices. |
| `GET` | `/api/devices/{deviceId}` | Get a device by ID. |
| `GET` | `/api/sensors` | List registered sensors. |
| `GET` | `/api/sensors/{sensorId}` | Get a sensor by ID. |
| `GET` | `/api/sensors/{sensorId}/measurements/climate?fromUtc={fromUtc}&toUtc={toUtc}&limit={limit}` | Get climate measurements in a time range. `limit` is optional and defaults to `100`; the supported range is `1` to `500`. `fromUtc` and `toUtc` must be valid timestamps. |

## Health checks

The health endpoints are intended for a container orchestrator or monitoring service. They are separate from the business API routes:

| Route | Checks | Use |
| --- | --- | --- |
| `/health/live` | The API process can respond. It does not check dependencies. | Liveness; detects a process that should be restarted. |
| `/health/ready` | PostgreSQL connectivity through the API's EF Core `DbContext`. | Readiness; indicates whether the API can currently serve requests that need the database. |

Both endpoints return `200` when their checks pass. Readiness returns `503` when PostgreSQL is unavailable. Liveness intentionally remains healthy during a database outage so a dependency failure does not cause the API process to be restarted repeatedly.

Local `dotnet run` (`https` profile):

```http
GET https://localhost:5001/health/live
```

```http
GET https://localhost:5001/health/ready
```

Docker Compose:

```http
GET http://localhost:8080/health/live
```

```http
GET http://localhost:8080/health/ready
```

The readiness check tests database connectivity; it does not verify that every expected migration has been applied.

## Project structure

```text
SmartHome.Api/
├── ErrorHandling/  # Global exception-to-ProblemDetails handling
├── Features/       # HTTP endpoints, request/response contracts, and mapping
├── Program.cs      # Host setup, dependency registration, middleware, and routes
└── SmartHome.Api.http
```

Endpoints translate HTTP input and Application outcomes into HTTP responses. Business rules belong in Domain and Application; persistence and database configuration belong in Infrastructure.
