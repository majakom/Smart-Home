# SmartHome.Infrastructure

Implements Application persistence interfaces with EF Core and PostgreSQL. Hosts provide the database connection string and register the layer:

```csharp
services.AddInfrastructure(connectionString);
```

The connection string key is `ConnectionStrings__SmartHome`. Supply it through the environment or a root `.env` file for local development.

Apply committed migrations with [SmartHome.DbMigrator](../SmartHome.DbMigrator/README.md). For EF CLI migration commands, see the [Persistence guide](Persistence/README.md).

## Structure and responsibilities

```text
SmartHome.Infrastructure/
├── Persistence/
│   ├── Adapters/       # EF implementations of Application interfaces
│   ├── Configurations/ # Entity, relationship, and index mappings
│   └── Migrations/     # Committed database schema changes
└── DependencyInjection.cs
```

`Persistence/` also contains the DbContext and EF design-time factory. `DependencyInjection.cs` configures PostgreSQL and registers the adapters. This project is a library and is not run directly.
