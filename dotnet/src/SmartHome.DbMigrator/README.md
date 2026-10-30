# SmartHome.DbMigrator

This one-shot executable applies committed EF Core migrations to PostgreSQL, then exits. If the required migration is already committed, just run the migrator below. Create a new migration with the [Persistence guide](../SmartHome.Infrastructure/Persistence/README.md) only when the database schema needs to change.

## Run locally

Prerequisites: .NET 10 SDK, PostgreSQL running, and a root `.env` with a valid `ConnectionStrings__SmartHome`. For a host run, the connection string should use `Host=localhost`.

From the repository root:

```powershell
docker compose up -d postgres
```

Then run the migrator:

```powershell
dotnet run --project dotnet/src/SmartHome.DbMigrator
```

The migrator applies any pending migrations and exits. If there are no pending migrations, the database is left as-is.

## Run in a container

Run the Compose service as a one-shot task. Choose whether to remove the stopped container:

Remove the container after it exits; output is shown in the terminal:

```powershell
docker compose run --build --rm db-migrator
```

Keep the stopped container so you can inspect its logs later:

```powershell
docker compose run --build db-migrator
```

Compose waits for PostgreSQL to become healthy and supplies `ConnectionStrings__SmartHome` with `Host=postgres`. The command builds the migrator image and applies pending migrations. With `--rm`, the stopped container is removed; without it, find the container with `docker compose ps -a` and read its output with `docker compose logs db-migrator`. The command's exit code is `0` on success and nonzero on failure.
