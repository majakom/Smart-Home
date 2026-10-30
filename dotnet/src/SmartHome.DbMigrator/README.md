# SmartHome.DbMigrator

This one-shot executable applies committed EF Core migrations to PostgreSQL, then exits. It does not generate migration files; use the [manual EF CLI guide](../SmartHome.Infrastructure/Persistence/README.md) to create migrations during development.

> **Before DbMigrator can apply a schema change, a developer must create its migration with `dotnet ef migrations add` and commit the generated files.** Run that command when the EF model changes; DbMigrator only applies migrations already in the project.

From the repository root, create a migration after changing the model:

```powershell
dotnet ef migrations add MigrationName `
  --project dotnet/src/SmartHome.Infrastructure `
  --startup-project dotnet/src/SmartHome.Infrastructure `
  --output-dir Persistence/Migrations
```

Replace `MigrationName` with a descriptive name, then review and commit the generated migration files and model snapshot.

## Run locally

Prerequisites: .NET 10 SDK, PostgreSQL running, and a root `.env` with a valid `ConnectionStrings__SmartHome`. For a host run, the connection string should use `Host=localhost`.

From the repository root:

```powershell
docker compose up -d postgres
dotnet run --project dotnet/src/SmartHome.DbMigrator
```

The migrator applies any pending migrations and exits. If there are no pending migrations, the database is left as-is.

## Run in a container

Run the Compose service as a one-shot task. Choose whether to remove the stopped container:

```powershell
# Remove the container after it exits; output is shown in the terminal.
docker compose up --build --rm db-migrator

# Keep the stopped container so you can inspect its logs later.
docker compose up --build db-migrator
```

Compose waits for PostgreSQL to become healthy and supplies `ConnectionStrings__SmartHome` with `Host=postgres`. The command builds the migrator image and applies pending migrations. With `--rm`, the stopped container is removed; without it, find the container with `docker compose ps -a` and read its output with `docker compose logs db-migrator`. The command's exit code is `0` on success and nonzero on failure.
