# Smart-Home

Local services are defined in the root `docker-compose.yml`. Run Docker Compose commands from the repository root.

## Configuration guides

- [PostgreSQL](infrastructure/postgres/README.md) — environment variables, startup, and persistent storage.
- [Mosquitto](infrastructure/mosquitto/README.md) — TLS certificates, broker credentials, and client connection tests.

Before starting the stack, copy `.env.example` to `.env` in the repository root and set `POSTGRES_PASSWORD`. The PostgreSQL guide describes the available environment variables. Configure Mosquitto certificates and credentials by following its guide.

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
