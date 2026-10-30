# Manual Database Migrations

> **This is a manual CLI workflow for local development.** The intended application/deployment path is the one-shot [SmartHome.DbMigrator](../../SmartHome.DbMigrator/README.md).

This project uses Entity Framework Core (EF Core) migrations to manage PostgreSQL schema changes. The `SmartHome.Infrastructure` project owns `SmartHomeDbContext`, its configurations, the design-time factory, and generated migrations.

## Prerequisites

- .NET 10 SDK
- PostgreSQL running and accessible when applying migrations
- EF Core CLI (`dotnet-ef`)
- A root `.env` file with `ConnectionStrings__SmartHome` configured

Install the EF Core CLI if needed, matching the `Microsoft.EntityFrameworkCore` version in [SmartHome.Infrastructure.csproj](../SmartHome.Infrastructure.csproj):

```powershell
dotnet tool install --global dotnet-ef --version <EF_CORE_VERSION>
```

Replace `<EF_CORE_VERSION>` with the version specified in `SmartHome.Infrastructure.csproj`.

## Create a migration

> **Create a migration whenever a model change needs a database schema change.** `database update` and DbMigrator can only apply migrations that have already been created.

Run from the repository root:

```powershell
dotnet ef migrations add MigrationName `
  --project dotnet/src/SmartHome.Infrastructure `
  --startup-project dotnet/src/SmartHome.Infrastructure `
  --output-dir Persistence/Migrations
```

Replace `MigrationName` with a descriptive name, such as `AddSensorStatus`. Review and commit the generated migration and model snapshot.

After creating the migration, you can apply it with [SmartHome.DbMigrator](../../SmartHome.DbMigrator/README.md) instead of running `dotnet ef database update` below. The migrator applies pending migrations and exits.

## Apply migrations

Start the local PostgreSQL service if needed (`docker compose up -d postgres`), then apply pending migrations:

```powershell
dotnet ef database update `
  --project dotnet/src/SmartHome.Infrastructure `
  --startup-project dotnet/src/SmartHome.Infrastructure
```

## Remove the last migration

If the latest migration has not been applied to the database:

```powershell
dotnet ef migrations remove `
  --project dotnet/src/SmartHome.Infrastructure `
  --startup-project dotnet/src/SmartHome.Infrastructure
```

If the latest migration has already been applied to your local development database, and you intend to undo its database changes as well as remove its files, use `--force`:

```powershell
dotnet ef migrations remove --force `
  --project dotnet/src/SmartHome.Infrastructure `
  --startup-project dotnet/src/SmartHome.Infrastructure
```

Check that you are connected to the intended local database first; reverting a migration can remove or alter data created by it. Do not remove migrations already applied to a shared database; add a corrective migration instead.

## Notes

- `SmartHomeDbContextFactory` creates the context for EF Core design-time commands and loads `ConnectionStrings__SmartHome` from the root `.env` file.
- `migrations add` generates migration files; `database update` connects to PostgreSQL and applies them.
- In normal development, prefer creating a new migration to undo a schema change.
- This CLI workflow applies migrations directly and does not use `SmartHome.DbMigrator`.
