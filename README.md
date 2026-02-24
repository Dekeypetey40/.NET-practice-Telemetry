# Telemetry API

.NET 8 Web API for instruments, runs (jobs), events, and alarms. Runs follow a strict state machine (Created → Queued → Running → Completed/Failed/Canceled) with validation and support bundle generation for investigations.

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
