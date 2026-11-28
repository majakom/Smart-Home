# .NET development workflows

This guide describes common code-change workflows. The architecture and current project status are in the [solution overview](README.md). Some projects are still scaffolds; those steps describe the intended design where implementation is incomplete.

## Add or change a database table

For a schema change, follow this order:

1. **Add or change the entity** in `SmartHome.Domain` (for example, under `Devices`, `Sensors`, or `Measurements`). Put business rules and domain types here; keep EF Core attributes and mapping out of Domain.
2. **Add or update the EF Core configuration** in `SmartHome.Infrastructure/Persistence/Configurations/` as an `IEntityTypeConfiguration<TEntity>`. Map the table, key, properties, conversions, and relationships here. `SmartHomeDbContext` scans this assembly for configurations; add a `DbSet<TEntity>` to the context if the entity needs a named set.
3. **Update application behavior if needed.** Add or change the use case and its persistence interface in Application, then implement that interface in Infrastructure. Skip this step if the table change does not affect an application use case.
4. **Add or update tests** for the changed behavior in the affected layers. Use PostgreSQL integration tests for database behavior.
5. **Create a migration** with `dotnet ef migrations add`, review the generated migration and model snapshot, and commit them. The exact command is in the [Persistence guide](SmartHome.Infrastructure/Persistence/README.md).
6. **Apply the migration** by running [SmartHome.DbMigrator](SmartHome.DbMigrator/README.md). It applies pending migrations and exits. For a local direct CLI update, use `dotnet ef database update` as described in the Persistence guide.

Only create a migration when the persisted schema changes. The Domain, Application use cases, Infrastructure persistence adapters, DbMigrator, and initial API operations are implemented. The Web project remains a template scaffold.

## Add a new telemetry measurement flow

For a new measurement category, implement the flow in each owning layer. For example, a new air-quality category would have its own Domain measurement, Application use case, persistence adapter, and MQTT handler.

1. **Add the Domain model** in `SmartHome.Domain`:  
   - Add the category to `Sensors/SensorCategory.cs`.  
   - Add the measurement entity and any related identifiers or value types under `Measurements/`.
2. **Add the Application workflow** in `SmartHome.Application`:
   - Add the request, result, validator, and use case under `UseCases/Telemetry/<UseCaseName>/`.
   - Add a persistence interface or extend existing one under `Abstractions/Telemetry/` for the operation the use case needs.
   - Reuse `UseCases/Telemetry/Shared/Validation/` and `Shared/Results/` where their existing rules and outcomes apply; keep category-specific rules with the new use case.
   - Register the use case and its validators in `SmartHome.Application/DependencyInjection.cs`.
3. **Implement persistence** in `SmartHome.Infrastructure`:
   - Add an adapter or extend existing one under `Persistence/Adapters/Telemetry/` that implements the Application interface.
   - Add an EF Core mapping under `Persistence/Configurations/`; `SmartHomeDbContext` discovers configurations from this assembly.
   - Register the adapter in `SmartHome.Infrastructure/DependencyInjection.cs`.
   - If the measurement changes the stored schema, create and review a migration and apply it using the database workflow above.
4. **Add the MQTT contract and route** in `SmartHome.TelemetryWorker`:
   - Add the incoming payload type under `Mqtt/Contracts/Incoming/Telemetry/`. Use explicit JSON property names and represent the message contract, not Domain entities.
   - Add the mapping for the new category in `Mqtt/Topics/Telemetry/MqttTelemetryCategories.cs`; the topic parser and category filters rely on this mapping.
   - Add a handler under `Mqtt/Receiving/Telemetry/Handlers/` implementing `ITelemetryMessageHandler` that deserializes the contract and maps it to the Application request. Keep business validation and persistence in Application and Infrastructure.
   - Register the handler in `SmartHome.TelemetryWorker/DependencyInjection.cs`.
   - Update `TelemetrySubscriber` subscription filters: it currently subscribes only to already existing categories.
5. **Test each layer** in its matching test project:
   - Domain invariants in `SmartHome.Domain.Tests/Measurements/`.
   - Use-case behavior and validation in `SmartHome.Application.Tests/UseCases/Telemetry/`.
   - EF mappings and persistence adapters in `SmartHome.Infrastructure.Tests/Persistence/`; use PostgreSQL integration tests for database behavior.
   - MQTT category mapping, filters, parsing, handler mapping, dispatcher selection, and subscription behavior in `SmartHome.TelemetryWorker.Tests/Mqtt/`.
   - Add a broker integration test only when verifying behavior that requires a real MQTT broker.
6. **Update the contracts and guides**:
   - Document the use case in `SmartHome.Application/UseCases/README.md`.
   - Document the topic, explicit category name, and payload shape in `SmartHome.TelemetryWorker/Mqtt/Topics/README.md`.

## Add a non-measurement application flow

Use this workflow for application behavior that is not recording telemetry, such as a future device-management operation. Add only the layers the behavior needs; a use case does not require a new Domain entity, database table, or host endpoint by default.

1. **Define the behavior**:
   - Identify the caller and the outcome the use case must provide.
   - Decide which rules belong to Domain and which coordination belongs to Application.
2. **Add Domain types only when needed** in `SmartHome.Domain`:
   - Add or update entities, value objects, and invariants in the relevant domain folder.
   - Keep HTTP, MQTT, UI, database, and dependency-injection concerns out of Domain.
3. **Implement the Application use case** in `SmartHome.Application`:
   - Add the request, result, validation, and use case under `UseCases/<Feature>/<UseCaseName>/`, following the existing use-case conventions.
   - Define an abstraction under `Abstractions/<Capability>/` for any operation the use case needs from an outer layer.
   - Keep the use case independent of transport and persistence implementations; register it and its validators in `SmartHome.Application/DependencyInjection.cs`.
4. **Implement outer-layer integrations only as required**:
   - If the use case reads or writes PostgreSQL, implement its Application abstractions in `SmartHome.Infrastructure/Persistence/Adapters/` and register them in Infrastructure DI.
   - Add EF Core configuration and a migration only if the stored schema changes; follow the database workflow above.
   - Add the entry point to the appropriate host. `SmartHome.Web` is still a scaffold; do not put use-case or business logic in a host.
   - Optionally, if the flow is triggered by MQTT and belongs to telemetry, add a contract and handler in `SmartHome.TelemetryWorker/Mqtt/` that maps the message to an Application request. Keep the Worker focused on telemetry; consider a separate host for unrelated MQTT commands.
5. **Test and document the flow**:
   - Test Domain rules in `SmartHome.Domain.Tests/` and use-case behavior in `SmartHome.Application.Tests/`.
   - Test Infrastructure adapters against PostgreSQL when the behavior depends on database behavior; test host or transport integration at the owning host layer.
   - Update `SmartHome.Application/UseCases/README.md` and any host-facing contract or guide affected by the new flow.

## Add an API endpoint

Use this workflow to expose an Application operation over HTTP. Implement the Application use case first if it does not already exist; follow the telemetry or non-measurement workflow above for that work.

1. **Define the HTTP contract**:
   - Choose the route, method, query or route parameters, response shape, and expected status codes.
   - Keep response types in the feature folder, for example `Features/<Feature>/<Feature>Response.cs` or `Features/<Feature>/<Subfeature>/<Subfeature>Response.cs`.
2. **Implement the endpoint**:
   - Place each operation in its own folder and endpoint file: `Features/<Feature>/<Operation>/<Operation>Endpoint.cs`.
   - For a nested feature, include its path in the folder structure: `Features/<Feature>/<Subfeature>/<Operation>/<Operation>Endpoint.cs`.
   - Bind and validate HTTP input, call the Application use case, and map its result to an HTTP response.
   - Keep business rules in Domain/Application and database access out of the endpoint.
3. **Add response mapping when needed**:
   - Place a mapper alongside its response type, following `<Feature>ResponseMapper.cs` or `<Subfeature>ResponseMapper.cs` naming.
   - Return the API's established validation, not-found, and Problem Details responses for corresponding outcomes.
4. **Register and describe the route**:
   - Register it in the feature route group, using `<Feature>Endpoints.cs`. Nested features can have their own `<Subfeature>Endpoints.cs` group under `Features/<Feature>/<Subfeature>/`.
   - Add OpenAPI metadata such as a name, summary, response types, and documented error responses.
   - For a new top-level route group, also map it from `SmartHome.Api/Program.cs`.
5. **Add or update API integration tests**:
   - Place tests under the matching feature and operation: `SmartHome.Api.Tests/Integration/Features/<Feature>/<Operation>Tests.cs`.
   - For a nested feature, mirror its source folder structure under `SmartHome.Api.Tests/Integration/Features/`.
   - Cover the successful response and relevant error cases, including invalid input and missing resources where applicable.
6. **Update API usage documentation**:
   - Add a request to `SmartHome.Api/SmartHome.Api.http` and document the route in `SmartHome.Api/README.md`.
   - Update Application use-case documentation only if the Application behavior also changed.

## Change a shared component

Use this workflow when changing behavior or a contract used by multiple features or layers, such as a shared validator, Application abstraction, result type, or transport option. Keep feature-specific behavior local rather than generalizing it prematurely.

1. **Identify ownership and consumers**:
   - Find the component's owning layer and all current callers before changing it.
   - Confirm the behavior is genuinely shared; if only one use case needs it, prefer keeping it with that use case.
2. **Make the change in the owning layer**:
   - Put business rules and shared domain concepts in `SmartHome.Domain`.
   - Put use-case coordination, shared validation, results, and persistence abstractions in `SmartHome.Application`.
   - Put database implementations and EF Core mappings in `SmartHome.Infrastructure`.
   - Keep MQTT contracts and routing in `SmartHome.TelemetryWorker`; map incoming data to Application types rather than moving business rules into the Worker.
3. **Update registrations and dependent implementations**:
   - Update the owning layer's `DependencyInjection.cs` when registrations change.
   - Update every implementation and consumer affected by a changed abstraction or contract; avoid adding dependencies from inner layers to outer layers.
4. **Test the shared behavior and its consumers**:
   - Add or update focused tests in the test projects corresponding to the affected layers.
   - Run tests for all affected consumers; use PostgreSQL or broker integration tests only when the change depends on those services.
5. **Update documentation when needed**:
   - Update the relevant use-case or service guide if the behavior, configuration, or external contract changes.
