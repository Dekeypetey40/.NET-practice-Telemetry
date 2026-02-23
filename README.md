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

1. Start Postgres:
   ```bash
   docker compose up -d
   ```

2. Apply migrations (from repo root):
   ```bash
   dotnet ef database update --project src/Telemetry.Infrastructure --startup-project src/Telemetry.Api
   ```
   If the above fails (e.g. missing Design package), run from the API directory with connection string:
   ```bash
   $env:ConnectionStrings__DefaultConnection="Host=localhost;Database=telemetry;Username=postgres;Password=postgres"
   dotnet ef database update --project ../Telemetry.Infrastructure
   ```

3. Run the API:
   ```bash
   dotnet run --project src/Telemetry.Api
   ```

4. Open Swagger: https://localhost:7xxx/swagger (port from launchSettings.json).

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

- **Migrations**: Ensure `DefaultConnection` in `appsettings.json` (or env) points to your Postgres. Use `dotnet ef database update --project src/Telemetry.Infrastructure --startup-project src/Telemetry.Api` from the repo root.
- **409 on queue/start/cancel**: Run is not in an allowed state; check `GET /runs/{id}` and the state machine rules.
- **Integration tests fail**: Docker must be running. On Windows, ensure Docker Desktop is started and the daemon is available (e.g. `npipe://./pipe/docker_engine`).

## CI

GitHub Actions (`.github/workflows/ci.yml`) runs on push/PR to `main`: restore → build → unit tests → integration tests (with Docker) → publish test results.
