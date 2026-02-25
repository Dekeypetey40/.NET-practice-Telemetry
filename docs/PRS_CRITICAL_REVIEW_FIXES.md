# Pull requests: Critical review fixes

Branches are pushed to `origin`. Create these PRs against `main` (e.g. via GitHub **Compare & pull request** or `gh pr create`). Apply in this order so main stays green; later branches may depend on earlier ones.

---

## 1. fix/security-config-validation (highest priority)

**Title:** `fix(security): require connection string, input validation, support bundle caps`

**Summary:** Addresses CRITICAL_REVIEW high-priority security and validation.

- **Secrets:** Require `ConnectionStrings:DefaultConnection`; remove hardcoded fallback.
- **Validation:** Add `[Required]`, `[MaxLength]` to `CreateInstrumentRequest` and `CreateRunRequest`; validate Method Parameters (max 50, key/value length) in `RunService`.
- **Support bundle:** Cap `lastLogEntries` 1–1000 in controller and service; use `run.Events` for timeline (single query).
- **README:** Security section (trusted/internal use, no auth).

---

## 2. fix/optimistic-concurrency

**Title:** `fix(concurrency): add optimistic concurrency for Run (Version token)`

**Summary:** Prevents concurrent state transitions from corrupting run state.

- Add `Version` (int) concurrency token to `Run`; increment on each state change.
- Migration `AddRunVersionConcurrencyToken` adds `Version` column to `Runs`.
- `RunRepository.SaveChangesAsync` catches `DbUpdateConcurrencyException` and throws `InvalidOperationException` (409).

---

## 3. fix/errors-and-cleanup

**Title:** `fix(cleanup): sanitize errors in prod, correlation ID abstraction, single-query timeline`

**Summary:** Safer errors in production and cleaner structure.

- **Errors:** In non-Development, return generic messages for 404/409/400 (no exception details to client).
- **SupportController:** Remove redundant `KeyNotFoundException` catch; let middleware handle.
- **Correlation ID:** `ICorrelationIdProvider` + `HttpContextCorrelationIdProvider`; `RunsController` uses it instead of middleware key.
- **Queries:** `RunService.GetTimelineAsync` and `SupportBundleService` use single query (load run with events, use `run.Events`).

---

## 4. fix/domain-state-guards

**Title:** `fix(domain): enforce state machine in Run entity (defense in depth)`

**Summary:** Domain entity rejects invalid transitions even if called incorrectly.

- `SetQueued`, `SetRunning`, `SetCompleted`, `SetFailed`, `SetCanceled` guard with `RunStateMachine.Can*` and throw `InvalidOperationException` when not allowed.

---

## 5. fix/ci-serilog

**Title:** `feat(observability): add Serilog with correlation ID in log context`

**Summary:** Structured logging with correlation ID for support and tracing.

- Add `Serilog.AspNetCore`; `UseSerilog` with `FromLogContext` and Console sink.
- `CorrelationIdMiddleware` pushes `CorrelationId` to `LogContext` for the request.
- `appsettings` Serilog section; `Log.CloseAndFlush` in `finally`.

**Note:** CI (TreatWarningsAsErrors, coverage upload) may already be on `main`; this branch adds Serilog only.

---

## Creating the PRs

1. Open the repo on GitHub.
2. Use **Pull requests** → **New pull request**.
3. Base: `main`, compare: the branch (e.g. `fix/security-config-validation`).
4. Use the title and summary above for the description.
5. Create the PR; merge in order 1 → 5 if you want a linear history.

After merging, run `git checkout main` and `git pull origin main` locally, then re-run tests and migrations as needed.
