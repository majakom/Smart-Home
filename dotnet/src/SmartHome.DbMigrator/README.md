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

The container must receive `ConnectionStrings__SmartHome` with `Host=postgres` to connect over the Compose network. Run the migrator as a one-shot task after PostgreSQL is healthy and before starting services that depend on the schema. A DbMigrator Dockerfile and Compose service have not been added yet.
