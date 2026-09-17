# Local CI Verification Gate (Mandatory)

An agent MUST NOT consider any implementation task, story, bug fix, or modification "done", and MUST NOT report completion or exit the turn, until all CI jobs have been executed and passed locally:

```bash
# POSIX / Devcontainer
scripts/verify-ci-local.sh

# Windows PowerShell 7+
scripts/verify-ci-local.ps1
```

Verification covers:
1. Stage 1: C# code format check (`dotnet format src/ControlEasyReborn.sln --verify-no-changes`)
2. Stage 2: Solution build in Release mode (`dotnet build -c Release`)
3. Stage 3: Unit and Architecture tests (`dotnet test tests/ControlEasyReborn.UnitTests`, `ArchitectureTests`)
4. Stage 4: Testcontainers MySQL integration tests (`dotnet test tests/ControlEasyReborn.IntegrationTests`)
5. Stage 5: Web production Docker build (`docker build -f docker/web.Dockerfile`)
6. Stage 6: OpenAPI client and swagger drift check (`ng-openapi-gen` vs `src/app/api`)

A modification is only done when `ci-local-passed.stamp` is generated and all 6 stages report `[PASS]`.
