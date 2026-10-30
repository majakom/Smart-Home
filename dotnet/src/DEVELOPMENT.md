# .NET development workflows

This guide describes common code-change workflows. The architecture and current project status are in the [solution overview](README.md). Some projects are still scaffolds; those steps describe the intended design where implementation is incomplete.

## Add or change a database table

For a schema change, follow this order:

1. **Add or change the entity** in `SmartHome.Domain` (for example, under `Devices`, `Sensors`, or `Measurements`). Put business rules and domain types here; keep EF Core attributes and mapping out of Domain.
2. **Add or update the EF Core configuration** in `SmartHome.Infrastructure/Persistence/Configurations/` as an `IEntityTypeConfiguration<TEntity>`. Map the table, key, properties, conversions, and relationships here. `SmartHomeDbContext` scans this assembly for configurations; add a `DbSet<TEntity>` to the context if the entity needs a named set.
3. **Update application behavior if needed.** Add or change the use case and its persistence interface in Application, then implement that interface in Infrastructure. Skip this step if the table change does not affect an application use case.
4. **Create a migration** with `dotnet ef migrations add`, review the generated migration and model snapshot, and commit them. The exact command is in the [Persistence guide](SmartHome.Infrastructure/Persistence/README.md).
5. **Apply the migration** by running [SmartHome.DbMigrator](SmartHome.DbMigrator/README.md). It applies pending migrations and exits. For a local direct CLI update, use `dotnet ef database update` as described in the Persistence guide.

Only create a migration when the persisted schema changes. Domain entities, EF configurations, and DbMigrator exist in the current code; Application and its use cases are still scaffolded.
