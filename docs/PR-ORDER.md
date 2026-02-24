# Open these PRs in order (piece by piece)

`origin/main` is currently **scaffold only**. Merge the following branches into `main` one at a time.

| # | Branch | Into | What it adds |
|---|--------|------|--------------|
| 1 | `pr/2-domain` | `main` | Domain layer + run state machine + unit tests |
| 2 | `pr/3-infrastructure` | `main` | EF Core PostgreSQL, DbContext, repos, migration |
| 3 | `pr/4-support-bundle` | `main` | Support bundle service (ZIP) |
| 4 | `pr/5-application` | `main` | Application services + DTOs |
| 5 | `pr/6-api` | `main` | API controllers + middleware + docker-compose |
| 6 | `pr/7-tests-ci-readme` | `main` | Integration tests, GitHub Actions CI, README |

**Links to create each PR (base = `main`, compare = branch):**

- PR 1: https://github.com/Dekeypetey40/.NET-practice-Telemetry/compare/main...pr/2-domain
- PR 2: https://github.com/Dekeypetey40/.NET-practice-Telemetry/compare/main...pr/3-infrastructure
- PR 3: https://github.com/Dekeypetey40/.NET-practice-Telemetry/compare/main...pr/4-support-bundle
- PR 4: https://github.com/Dekeypetey40/.NET-practice-Telemetry/compare/main...pr/5-application
- PR 5: https://github.com/Dekeypetey40/.NET-practice-Telemetry/compare/main...pr/6-api
- PR 6: https://github.com/Dekeypetey40/.NET-practice-Telemetry/compare/main...pr/7-tests-ci-readme

Merge PR 1, then open and merge PR 2, and so on. (Each PR is based on the previous state of `main`.)
