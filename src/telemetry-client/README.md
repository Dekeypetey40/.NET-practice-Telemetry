# Telemetry Angular client

Browser UI for the Telemetry API (runs, instruments, state transitions, health).

Architecture, API design, and full-stack setup live in the [root README](../../README.md).

## Run (dev)

Requires the API at `http://localhost:5244` (see root README).

```bash
npm install
npm start
```

Open **http://localhost:4200**. Requests to `/api/*` are proxied to the API (`proxy.conf.json`).

## Build

```bash
npm run build
```

## E2E (Playwright)

API must be running:

```bash
npm run e2e:install   # first time only
npm run e2e
```

Or use full-stack Compose from the repo root (`docker compose up --build`); CI runs Playwright against that stack.
