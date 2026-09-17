---
description: "Mandatory local CI verification gate for all modifications"
always_on: true
---

# Mandatory Local CI Verification Gate

All agents working on ControlEasy Reborn MUST obey the following strict policy:

1. **No Modification Done Without Local CI Passing:**
   No task, bugfix, or modification may be considered "done", and no task checkbox in `.specs/<feature>/tasks.md` may be marked `[X]`, until all jobs in the CI pipeline pass locally:
   ```bash
   ./scripts/verify-ci-local.sh
   # Or: make verify-ci
   ```

2. **What the Local CI Gate Checks:**
   - **Stage 1 (Format):** `dotnet format src/ControlEasyReborn.sln --verify-no-changes`
   - **Stage 2 (Build):** `dotnet build src/ControlEasyReborn.sln --configuration Release`
   - **Stage 3 (Fast Tests):** `dotnet test tests/ControlEasyReborn.UnitTests` and `ArchitectureTests`
   - **Stage 4 (Integration Tests):** `dotnet test tests/ControlEasyReborn.IntegrationTests` (Testcontainers MySQL)
   - **Stage 5 (Web Build):** Angular production build via Docker (`docker build -f docker/web.Dockerfile .`)
   - **Stage 6 (OpenAPI Drift):** In-process API launch, swagger generation, `ng-openapi-gen`, and `git diff --exit-code -- src/app/api`

3. **Lifecycle Stop Hook:**
   The Stop hook in `.agent/hooks.json` automatically blocks the agent from finishing if code modifications exist and `scripts/verify-ci-local.sh` has not passed.
