# Telemetry API

.NET 8 Web API for instruments, runs (jobs), events, and alarms. Runs follow a strict state machine (Created → Queued → Running → Completed/Failed/Canceled) with validation and support bundle generation for investigations.

## IMPORTANT TODO:
## 1. Security

### 1.1 Authentication and Authorization

- **Finding:** No authentication or authorization is implemented. All endpoints are anonymous.
- **Risk:** Any client can create instruments, create/queue/start/cancel runs, and download support bundles. In a multi-tenant or production context this is unacceptable.
- **Recommendation:** Add authentication (e.g. JWT or API keys) and authorization (e.g. roles or resource-based policies). At minimum, document that the API is intended for trusted/internal use only and add a security section to the README.

### 1.2 Secrets and Configuration

- **Finding:** `Program.cs` uses a hardcoded connection string fallback when `DefaultConnection` is missing:
  ```csharp
  var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
      ?? "Host=localhost;Database=telemetry;Username=postgres;Password=postgres";
  ```
- **Risk:** Default credentials can be committed or leaked; production could accidentally use fallback if config is misconfigured.
- **Recommendation:** Do not fall back to a default connection string. Require `ConnectionStrings:DefaultConnection` (or throw at startup). Use User Secrets or environment variables for local dev and never commit credentials.

### 1.3 Information Disclosure in Errors

- **Finding:** `ExceptionHandlingMiddleware` returns the exception message to the client for `KeyNotFoundException` (404), `InvalidOperationException` (409), and `ArgumentException` (400). For example, "Run is in state Running; cannot start. Only Queued runs can be started."
- **Risk:** In production, detailed messages can reveal internal state and business rules, aiding enumeration or abuse.
- **Recommendation:** In non-Development environments, return generic messages (e.g. "Resource not found", "Conflict", "Invalid request") and log the full exception server-side. Use a small set of error codes or types if the client needs to distinguish cases.

### 1.4 Input Validation and Payload Limits

- **Finding:** Request DTOs (`CreateRunRequest`, `CreateInstrumentRequest`) have no validation attributes. Validation exists only in domain (e.g. `SampleId.Create`, `Instrument.Create`), so invalid data is rejected only after reaching the application layer.
- **Risk:** Oversized or malformed input can cause unnecessary load, and very large payloads (e.g. `CreateRunRequest.Parameters` as an unbounded dictionary) could be used for DoS or storage abuse.
- **Recommendation:** Add API-level validation: `[Required]`, `[MaxLength]`, `[Range]` on DTOs. Enforce maximum size for `Parameters` (e.g. key/value length and total count). Consider `[FromBody]` size limits (e.g. `RequestSizeLimit`) to cap request body size.

### 1.5 Support Bundle and DoS

- **Finding:** `SupportController.CreateSupportBundle` accepts `lastLogEntries` as a query parameter with default 100 and no upper bound. `SupportBundleService.CreateBundleForRunAsync` builds the entire ZIP in a single `MemoryStream`.
- **Risk:** A client can request `lastLogEntries=10000000`, causing high memory usage and potential OOM. Large timelines also increase memory for the in-memory ZIP.
- **Recommendation:** Cap `lastLogEntries` (e.g. max 1000) in the API and optionally in the service. Consider streaming the ZIP (e.g. `ZipArchive` with streaming entries) or at least bounding the total size of log/timeline data included.

### 1.6 AllowedHosts and CORS

- **Finding:** `appsettings.json` has `"AllowedHosts": "*"`.
- **Risk:** Reduces defense in depth against host header misuse if the app uses host-based behavior.
- **Recommendation:** Restrict to known hosts in production or document why `*` is acceptable. If the API is consumed by a SPA or dashboard, configure CORS explicitly instead of leaving defaults.

---

## 2. Code Cleanliness and Consistency

### 2.1 Redundant Exception Handling

- **Finding:** `SupportController.CreateSupportBundle` catches `KeyNotFoundException` and returns 404, while `ExceptionHandlingMiddleware` already maps `KeyNotFoundException` to 404.
- **Risk:** Duplicate behavior; if middleware logic changes, the controller catch can become inconsistent or dead code.
- **Recommendation:** Remove the try/catch in the controller and let the middleware handle it. Use a single, consistent strategy for exception-to-HTTP mapping.

### 2.2 Correlation ID Coupling

- **Finding:** `RunsController` reads the correlation ID from `HttpContext.Items[CorrelationIdMiddleware.ItemKey]`, depending on the middleware’s public constant.
- **Risk:** Tight coupling between API and middleware; refactoring the middleware (e.g. changing key or using a different mechanism) requires controller changes.
- **Recommendation:** Introduce an abstraction (e.g. `ICorrelationIdProvider` or `IHttpContextAccessor`-based helper in a shared API contract) so controllers do not depend on middleware implementation details.

### 2.3 Duplicate Data Access in Support Bundle

- **Finding:** `SupportBundleService.CreateBundleForRunAsync` loads the run with `includeEvents: true`, then calls `_runRepository.GetTimelineAsync(runId, cancellationToken)`. The timeline is already available as `run.Events`.
- **Risk:** Redundant database query and unnecessary load; slight inconsistency (two sources for the same logical data).
- **Recommendation:** Use `run.Events` for the timeline when run was loaded with `includeEvents: true`; do not call `GetTimelineAsync` in that path.

### 2.4 GetTimeline Double Query

- **Finding:** `RunService.GetTimelineAsync` calls `GetByIdAsync(runId, includeEvents: false)` to check existence and get run id, then `GetTimelineAsync(runId)` for events.
- **Risk:** Two round-trips where one would suffice (e.g. get run with events, or get timeline and derive run id from first event if needed).
- **Recommendation:** Either load run with `includeEvents: true` and map `run.Events` to the response, or have a single repository method that returns timeline (and optionally run) in one query. Prefer one round-trip for a single logical operation.

### 2.5 Plan vs Implementation: Serilog

- **Finding:** The plan mentions correlation IDs and Serilog-enriched logs. The API project has no Serilog package or configuration in `Program.cs`.
- **Risk:** Logs are not enriched with correlation ID, reducing the value of the support bundle and traceability.
- **Recommendation:** Add Serilog, enrichers for correlation ID (and request path, etc.), and use it as the logging provider so support bundle log collection can filter by correlation ID as intended.

---

## 3. Maintainability

### 3.1 No API Versioning

- **Finding:** All routes are unversioned (e.g. `/runs`, `/instruments`).
- **Risk:** Future breaking changes force a single big-bang upgrade or ambiguous behavior for different clients.
- **Recommendation:** Introduce API versioning (e.g. URL path or query) early and document the versioning strategy. Even a single version (e.g. `v1`) sets the pattern.

### 3.2 DTOs and Validation Location

- **Finding:** Validation lives in domain value objects and entities (e.g. `SampleId.Create`, `Instrument.Create`). API layer has no explicit validation attributes.
- **Risk:** Invalid requests are only rejected after entering the application layer; API contract (required fields, max lengths) is not self-documenting or enforceable at the edge.
- **Recommendation:** Add validation attributes to request DTOs and consider FluentValidation or a small validation layer at the API boundary so invalid requests are rejected with clear 400 responses and OpenAPI reflects constraints.

### 3.3 Domain State Transitions Not Self-Enforcing

- **Finding:** `Run.SetQueued`, `SetRunning`, `SetCanceled`, etc. do not check the current state. They rely on `RunService` calling `RunStateMachine.CanQueue(run.CurrentState)` (etc.) before invoking the method.
- **Risk:** If the repository or another caller mutates a run directly, the entity can transition to an invalid state; the domain does not defend itself.
- **Recommendation:** Add guards in the entity methods (e.g. `if (!RunStateMachine.CanQueue(CurrentState)) throw ...`) so the domain is the single source of truth and misuse fails fast inside the entity.

### 3.4 WPF Dashboard Placeholder

- **Finding:** `MainWindow.xaml.cs` is effectively empty (only `InitializeComponent()`). The plan calls for a dashboard that lists runs and triggers queue/start/cancel.
- **Risk:** The “full-stack” and FlaUI automation story is incomplete; the dashboard does not yet demonstrate API integration or UX.
- **Recommendation:** Implement at least a minimal dashboard (list runs, trigger actions, show state) and wire it to the API so the architecture is demonstrable and testable with FlaUI.

### 3.5 BDD and UI Tests Stubs

- **Finding:** `Telemetry.BddTests` and `Telemetry.UiTests` contain placeholder `UnitTest1`-style files rather than SpecFlow scenarios or FlaUI tests.
- **Risk:** Test pyramid and “BDD for run lifecycle” / “FlaUI for WPF” from the plan are not yet realized.
- **Recommendation:** Add at least one SpecFlow scenario for the run lifecycle and one FlaUI smoke test for the dashboard when the dashboard exists, and remove or replace placeholder tests.

---

## 4. Scalability and Robustness

### 4.1 No Optimistic Concurrency on Runs

- **Finding:** `Run` has no concurrency token (e.g. `RowVersion` or timestamp). Queue/Start/Cancel load the run, mutate it, and call `SaveChangesAsync`.
- **Risk:** Concurrent requests (e.g. two “Start” calls for the same run, or Queue and Cancel at the same time) can result in last-write-wins and inconsistent or invalid state (e.g. double start).
- **Recommendation:** Add a concurrency token to `Run` and handle `DbUpdateConcurrencyException` in the application layer (retry or return 409 with a clear message). This is important for any multi-user or automated scenario.

### 4.2 Support Bundle Built Fully In Memory

- **Finding:** The support bundle is built with `new MemoryStream()` and the entire ZIP is written to it before returning. The stream is then returned to the client.
- **Risk:** For runs with very large timelines or log sets, memory usage can spike and cause OOM or slow GC, especially under concurrent bundle requests.
- **Recommendation:** Cap the amount of data included (as in 1.5) and consider streaming the ZIP response (e.g. `PushStreamContent` or similar) so the buffer does not hold the entire file in memory at once.

### 4.3 No Pagination or List Endpoints

- **Finding:** There are no list endpoints (e.g. list runs for an instrument, list instruments). Only get-by-id and create/transition actions exist.
- **Risk:** When list endpoints are added later, returning unbounded lists will not scale; adding pagination later is a breaking change for clients.
- **Recommendation:** When adding list endpoints, design them with pagination (e.g. `limit`/`offset` or `pageSize`/`continuationToken`) from the start. Consider cursor-based pagination for consistency under insert/update.

### 4.4 Log Collector Optional and Not Wired

- **Finding:** `ISupportBundleLogCollector` is optional in `SupportBundleService`; it is not registered in `ServiceCollectionExtensions` (Infrastructure). So logs are never included in the bundle.
- **Risk:** Support bundle does not fulfill the “logs filtered by correlation ID” story; the feature is partially implemented.
- **Recommendation:** Implement and register a log collector (e.g. in-memory ring buffer or file-based, with correlation ID filtering) so support bundles actually include logs. Document the strategy (e.g. in-memory for dev, external sink for production).

---

## 5. CI/CD and Tooling

### 5.1 .NET Version Mismatch

- **Finding:** The plan specifies .NET 9. The solution uses `net8.0` (e.g. `Telemetry.Api.csproj`) and the GitHub Actions workflow uses `dotnet-version: "8.0.x"`.
- **Risk:** Portfolio and docs say “current” .NET 9, but build and runtime are on 8. Inconsistent for reviewers and future upgrades.
- **Recommendation:** Align either on .NET 8 everywhere (and update the plan) or upgrade to .NET 9 and set the workflow to 9.0.x.

### 5.2 Warnings as Errors Not Enforced

- **Finding:** The plan says “warnings as errors where appropriate.” The CI build step uses `-p:TreatWarningsAsErrors=false`.
- **Risk:** New warnings can accumulate; code quality bar is lower than stated.
- **Recommendation:** Set `TreatWarningsAsErrors=true` in CI (and in project files if desired). Fix existing warnings first, then keep the build strict.

### 5.3 No Code Coverage in CI

- **Finding:** The plan calls for “publish test results and coverage.” The workflow publishes test results (e.g. trx) but has no coverage collection or upload (e.g. coverlet + Codecov/summary).
- **Risk:** Coverage is not visible in PRs or over time; hard to enforce or improve test coverage.
- **Recommendation:** Add coverlet (or similar) to test projects, generate coverage in CI, and publish it (e.g. as artifact or to a coverage service). Optionally add a coverage gate for critical projects.

---

## Architecture

- **Telemetry.Api** – Controllers, middleware (correlation ID, exception handling)
- **Telemetry.Application** – Services, DTOs (RunService, InstrumentService)
- **Telemetry.Domain** – Entities, RunStateMachine, value objects
- **Telemetry.Infrastructure** – EF Core (PostgreSQL), repositories, support bundle service
- **Tests** – Unit (state machine, domain), Integration (WebApplicationFactory + Testcontainers)

## Run state machine

```
Created → Queued → Running → (Completed | Failed | Canceled)
   ↓         ↓         ↓
Canceled  Canceled  Canceled
```

Invalid transitions (e.g. start when already Completed) return **409 Conflict** with a clear error message.

## Prerequisites

- .NET 8 SDK
- Docker (for local Postgres and for integration tests)

## Run locally

1. Start Postgres (must be running before migrations):
   ```bash
   docker compose up -d
   ```
   Wait a few seconds for the container to be ready.

2. Apply migrations (from repo root):
   ```bash
   dotnet ef database update --project src/Telemetry.Infrastructure --startup-project src/Telemetry.Api
   ```
   The app uses **port 5433** for Postgres (Docker maps `5433:5432`) to avoid conflicting with a local PostgreSQL on 5432. If you get **password authentication failed**, ensure you're not overriding the connection string with port 5432; use `Port=5433` or rely on `appsettings.json`.

3. Run the API:
   ```bash
   dotnet run --project src/Telemetry.Api
   ```

4. Open Swagger: **http://localhost:5244/swagger** (default `http` profile). For HTTPS use the `https` launch profile: `dotnet run --project src/Telemetry.Api --launch-profile https`, then https://localhost:7254/swagger.

## Core endpoints

| Method | Endpoint | Description |
|--------|----------|-------------|
| POST | /instruments | Create instrument |
| GET | /instruments/{id}/health | Instrument health + alarms |
| POST | /runs | Create run (sample + method metadata) |
| POST | /runs/{id}/queue | Queue run |
| POST | /runs/{id}/start | Start run |
| POST | /runs/{id}/cancel | Cancel run |
| GET | /runs/{id} | Run state + metadata |
| GET | /runs/{id}/timeline | Ordered event timeline |
| POST | /runs/{id}/support-bundle | Download ZIP (metadata, timeline, env, optional logs) |

## Testing

- **Unit tests** (no Docker):
  ```bash
  dotnet test tests/Telemetry.UnitTests/Telemetry.UnitTests.csproj
  ```

- **Integration tests** (require Docker):
  ```bash
  dotnet test tests/Telemetry.IntegrationTests/Telemetry.IntegrationTests.csproj
  ```

- **All tests**:
  ```bash
  dotnet test
  ```

## Reproduce an incident / support bundle

1. Create a run and progress it (queue, start, etc.).
2. Call `GET /runs/{id}/timeline` to inspect the ordered event list.
3. Call `POST /runs/{id}/support-bundle` to download a ZIP with:
   - `metadata.json` – run state and metadata
   - `timeline.json` – events in order
   - `environment.json` – app version and environment
   - `logs.txt` – if a log collector is registered

Use the same `X-Correlation-Id` header across calls to trace a run in logs.

## Troubleshooting

- **28P01 / password authentication failed for user "postgres"**: The EF command is connecting to a Postgres that doesn’t accept `postgres`/`postgres`. Start the project’s Postgres with `docker compose up -d` and ensure nothing else is using port 5432 (app expects 5433 for Docker), or set `ConnectionStrings__DefaultConnection` to your actual host, user, and password (see “Apply migrations” above).
- **Migrations**: Ensure `DefaultConnection` in `appsettings.json` (or env) points to your Postgres. Use `dotnet ef database update --project src/Telemetry.Infrastructure --startup-project src/Telemetry.Api` from the repo root.
- **409 on queue/start/cancel**: Run is not in an allowed state; check `GET /runs/{id}` and the state machine rules.
- **Integration tests fail**: Docker must be running. On Windows, ensure Docker Desktop is started and the daemon is available (e.g. `npipe://./pipe/docker_engine`).

## CI

GitHub Actions (`.github/workflows/ci.yml`) runs on push/PR to `main`: restore → build → unit tests → integration tests (with Docker) → publish test results.
