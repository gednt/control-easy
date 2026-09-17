---
paths:
  - "**/*"
---

Follow AGENTS.md.

## Mandatory Completion Gate
No task, bugfix, or modification is considered "done" until all jobs in the CI pipeline pass locally (`scripts/verify-ci-local.sh` or `make verify-ci`). Per-turn stop hooks enforce fast checks (`--fast`: format, build, unit + arch tests in ~3s). CI tasks that download things inside Docker (Testcontainers MySQL and Docker web build) run once per task completion, at the end of all tasks completions, not after every turn. Agents MUST NOT mark tasks complete, push, or report done without running and passing the full local CI gate.

**All dotnet/npx commands run inside Docker containers — `docker` CLI is the ONLY host requirement.** The verify scripts use `mcr.microsoft.com/dotnet/sdk:8.0`, `mcr.microsoft.com/dotnet/aspnet:8.0`, and `node:20-slim` images via `docker run`. Never install .NET SDK, Node.js, or npm on the host machine.