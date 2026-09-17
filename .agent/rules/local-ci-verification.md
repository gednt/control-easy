---
description: "Mandatory local CI verification gate for all modifications"
always_on: true
---

# Mandatory Local CI Verification Gate

All agents working on ControlEasy Reborn MUST obey the following strict policy:

1. **Two-Tier Verification Cadence:**
   - **Per-Turn Fast Gate (`scripts/verify-ci-local.sh --fast`):** Enforced automatically on agent stop after conversational turns where code files are modified. Runs format check, Release build, and unit/architecture tests with ZERO Docker downloads (named NuGet/npm cache volumes are reused after the first pull).
   - **Task Completion Full Gate (`scripts/verify-ci-local.sh`):** Runs the full 6-stage suite: format, build, unit & architecture tests, Testcontainers MySQL integration tests, Angular Docker production build, and OpenAPI client drift check.

2. **All dotnet/npx commands run inside Docker containers — docker CLI is the only host requirement:**
   The CI scripts (`scripts/verify-ci-local.sh` and `scripts/verify-ci-local.ps1`) MUST NOT call `dotnet`, `npx`, `ng`, or any other language SDK directly on the host machine. All such commands execute via `docker run` using the following images:
   - `mcr.microsoft.com/dotnet/sdk:8.0` — build, format, and test stages
   - `mcr.microsoft.com/dotnet/aspnet:8.0` — swagger generation (stage 6)
   - `node:20-slim` — `ng-openapi-gen` drift check (stage 6)
   Named volumes `ce-nuget-packages` and `ce-npm-cache` persist downloaded packages between runs so subsequent fast checks take seconds, not minutes.

3. **Docker Download Tasks Run Only Once Per Task Completion:**
   The CI tasks that need to pull Docker images or start Testcontainers (Testcontainers MySQL integration tests and Angular Docker production build) run **only once per task completion, at the end of all tasks completions, not after every turn**.

4. **No Task Completed Without Full Local CI Passing:**
   No task, bugfix, or modification may be considered "done", and no task checkbox in `.specs/<feature>/tasks.md` may be marked `[X]`, until all jobs in the CI pipeline pass locally:
   ```bash
   ./scripts/verify-ci-local.sh
   # Or: make verify-ci
   # Or on Windows PowerShell: ./scripts/verify-ci-local.ps1
   ```

5. **Lifecycle Stop Hooks:**
   Stop hooks automatically enforce fast checks on intermediate conversational turns, and block agents from completing or committing tasks if the full verification gate has not passed at task completion. Untracked files are strictly included in status fingerprinting. Pre-commit full verification results automatically roll over upon `git commit`, eliminating duplicate executions of slow Docker tasks.
