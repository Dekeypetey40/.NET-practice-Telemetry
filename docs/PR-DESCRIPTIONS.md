# Suggested PR titles and descriptions

Copy the title and description below when creating each pull request.

---

## PR 1: Domain layer

**Branch:** `pr/2-domain` → `main`

**Title:**  
`feat(domain): add entities, run state machine, and unit tests`

**Description:**
```markdown
## Summary
Introduces the domain layer: entities, value objects, and the run state machine.

## Changes
- **Entities:** `Instrument`, `Run`, `RunEvent`, `Alarm` with private setters and factory methods
- **Value objects:** `SampleId`, `MethodMetadata`
- **Run state machine:** `RunState` enum and `RunStateMachine` with allowed transitions (Created → Queued → Running → Completed/Failed/Canceled) and helpers (`CanQueue`, `CanStart`, `CanCancel`, `IsTerminal`)
- **Unit tests:** xUnit + FluentAssertions for state machine transitions and domain behavior (24 tests)

## Testing
- `dotnet test tests/Telemetry.UnitTests/Telemetry.UnitTests.csproj`
```

---

## PR 2: Infrastructure (EF Core + repositories)

**Branch:** `pr/3-infrastructure` → `main`

**Title:**  
`feat(infrastructure): add EF Core PostgreSQL, DbContext, repositories, and initial migration`

**Description:**
```markdown
## Summary
Adds persistence with EF Core and PostgreSQL, plus repository implementations.

## Changes
- **DbContext:** `TelemetryDbContext` with `Instruments`, `Runs`, `RunEvents`, `Alarms`
- **Entity configurations:** Fluent API for tables, keys, and relationships (including Run → RunEvents backing field)
- **Repositories:** `IRunRepository` / `RunRepository`, `IInstrumentRepository` / `InstrumentRepository` (interfaces in Application)
- **Migrations:** Initial migration for PostgreSQL; design-time factory for `dotnet ef`
- **DI:** `AddTelemetryInfrastructure(connectionString)` in Infrastructure

## Testing
- Build: `dotnet build src/Telemetry.Infrastructure/Telemetry.Infrastructure.csproj`
- Migrations: `dotnet ef migrations add <Name> --project src/Telemetry.Infrastructure --startup-project src/Telemetry.Api`
```

---

## PR 3: Support bundle service

**Branch:** `pr/4-support-bundle` → `main`

**Title:**  
`feat(infrastructure): add support bundle service (ZIP with metadata, timeline, environment, optional logs)`

**Description:**
```markdown
## Summary
Implements the support-bundle feature: a ZIP artifact for investigations.

## Changes
- **Contracts:** `ISupportBundleService`, `ISupportBundleLogCollector` (optional) in Application
- **Implementation:** `SupportBundleService` in Infrastructure builds a ZIP containing:
  - `metadata.json` – run state and metadata
  - `timeline.json` – ordered events
  - `environment.json` – app version, environment, machine, timestamp
  - `logs.txt` – last N entries when a log collector is registered
- **Registration:** `ISupportBundleService` registered in `AddTelemetryInfrastructure`
```

---

## PR 4: Application layer

**Branch:** `pr/5-application` → `main`

**Title:**  
`feat(application): add DTOs, RunService, InstrumentService, and DI extension`

**Description:**
```markdown
## Summary
Adds the application layer: DTOs and services that orchestrate the domain and enforce the run state machine.

## Changes
- **DTOs:** `CreateInstrumentRequest`, `CreateRunRequest`, `RunResponse`, `RunTimelineResponse`, `InstrumentHealthResponse`, `AlarmResponse`
- **RunService:** Create, Queue, Start, Cancel, GetById, GetTimeline; throws `InvalidOperationException` (409) for invalid transitions and `KeyNotFoundException` (404) for missing entities
- **InstrumentService:** Create, GetHealth (with alarms)
- **DI:** `AddTelemetryApplication()` registers services
```

---

## PR 5: API layer

**Branch:** `pr/6-api` → `main`

**Title:**  
`feat(api): add Instruments, Runs, Support controllers; middleware; docker-compose`

**Description:**
```markdown
## Summary
Exposes the application via REST API with Controllers and middleware.

## Changes
- **Controllers:**
  - `InstrumentsController`: `POST /instruments`, `GET /instruments/{id}/health`
  - `RunsController`: `POST /runs`, `POST /runs/{id}/queue`, `POST /runs/{id}/start`, `POST /runs/{id}/cancel`, `GET /runs/{id}`, `GET /runs/{id}/timeline`
  - `SupportController`: `POST /runs/{id}/support-bundle`
- **Middleware:** Correlation ID (`X-Correlation-Id`), global exception handling (404/409/400/500)
- **Setup:** `Program.cs` wires Application + Infrastructure and connection string from config
- **Docker:** `docker-compose.yml` for PostgreSQL 16 for local run
```

---

## PR 6: Integration tests, CI, and README

**Branch:** `pr/7-tests-ci-readme` → `main`

**Title:**  
`test(docs): add integration tests, GitHub Actions CI, and README`

**Description:**
```markdown
## Summary
Adds integration tests (Testcontainers + WebApplicationFactory), GitHub Actions CI, and project documentation.

## Changes
- **Integration tests:**
  - `TelemetryAppFactory` with config override for Testcontainers connection string
  - `IntegrationTestFixture` (IAsyncLifetime): starts Postgres container, runs migrations, creates client
  - `RunsControllerTests`: full run lifecycle (create → queue → start → get → timeline) and 409 when starting from Created
- **CI:** `.github/workflows/ci.yml` – restore, build, unit tests, integration tests (Docker), publish test results
- **README:** Architecture, state machine, endpoints, run locally (Docker Compose + migrations), testing (unit vs integration), support bundle usage, troubleshooting
- **Docs:** `docs/PR-ORDER.md` – PR merge order and compare links
```

---

## Quick reference

| PR | Branch        | Title (short) |
|----|---------------|----------------|
| 1  | pr/2-domain   | feat(domain): entities, state machine, unit tests |
| 2  | pr/3-infrastructure | feat(infrastructure): EF Core, repos, migration |
| 3  | pr/4-support-bundle  | feat(infrastructure): support bundle service |
| 4  | pr/5-application     | feat(application): DTOs, RunService, InstrumentService |
| 5  | pr/6-api             | feat(api): controllers, middleware, docker-compose |
| 6  | pr/7-tests-ci-readme | test(docs): integration tests, CI, README |
