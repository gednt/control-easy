# Mandatory Local CI Verification Gate

All agents working on ControlEasy Reborn MUST obey the following strict policy:

1. **Two-Tier Verification Cadence:**
   - **Per-Turn Fast Gate (`scripts/verify-ci-local.sh --fast`):** Enforced automatically on agent stop after conversational turns where code files are modified. Runs format check (`dotnet format --verify-no-changes`), Release build, and unit/architecture tests in ~3 seconds with ZERO Docker downloads and ZERO containers.
   - **Task Completion / End of All Tasks Full Gate (`scripts/verify-ci-local.sh`):** Runs the full 6-stage suite: format, build, unit & architecture tests, Testcontainers MySQL integration tests, Angular Docker production build, and OpenAPI client drift check.

2. **Docker Download Tasks Run Only Once Per Task Completion:**
   The CI tasks that need to download things inside Docker (Testcontainers MySQL integration tests and Angular Docker production build) run **only once per task completion, at the end of all tasks completions, not after every turn**.

3. **No Task Completed Without Full Local CI Passing:**
   No task, bugfix, or modification may be considered "done", and no task checkbox in `.specs/<feature>/tasks.md` may be marked `[X]`, until all jobs in the CI pipeline pass locally:
   ```bash
   ./scripts/verify-ci-local.sh
   # Or: make verify-ci
   # Or on Windows PowerShell: ./scripts/verify-ci-local.ps1
   ```

4. **Lifecycle Stop Hooks:**
   Stop hooks automatically enforce fast checks on intermediate conversational turns, and block agents from completing or committing tasks if the full verification gate has not passed at task completion.
