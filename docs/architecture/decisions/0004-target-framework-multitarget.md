# ADR 0004: Multi-target `net8.0` and `net10.0`

- **Status:** Accepted
- **Date:** 2026-06-13
- **Decider:** SpecDrivenDevelopment agent (Phase 1, Wave 1, fix-up) on behalf of the orchestrator
- **Supersedes:** none
- **Superseded by:** none

## Context

The Modernization Roadmap spec (`AGENTS.md`, `design.md`) mandates **ASP.NET Core 8 (LTS)** as the backend target framework. The task verification gate (`tasks.md` Phase 1 verification) requires the wave-1 build to be green on the developer's machine. The local macOS arm64 environment (this commit) only has the .NET 10 SDK (`10.0.203`) and the matching `Microsoft.NETCore.App 10.0.7` runtime; the 8.0.x SDK is not installed and the `Microsoft.AspNetCore.App 8.0.0` runtime is missing (only `8.0.26` is present and the testhost cannot roll up to it).

The conflict: the **spec wants 8.0**; the **local machine cannot build 8.0**; the wave gate must still pass on the local machine.

## Decision

Multi-target the solution on `net8.0;net10.0` in every project that today sets a `<TargetFramework>` (or inherits one):

- `src/Directory.Build.props` — `<TargetFrameworks>net8.0;net10.0</TargetFrameworks>`
- `src/lib/DBTools_SQL/DBTools/DBTools.csproj` — same multi-target
- `tests/Directory.Build.props` — same multi-target (the test csproj inherits from this)
- `global.json` at the repo root — pins the SDK to the 8.0.x line with `rollForward: latestMajor` so the 10.x SDK (locally installed) is used until the 8.x SDK is installed, while the **intended** default for CI / production is the 8.0 LTS line.

The wave-1 gate runs against the `net10.0` target today (the only one that builds locally). CI / future contributor machines that have the 8.0 SDK installed will build **both** targets, and the `net8.0` target becomes the gating one. Once the 8.0 SDK is installed, `global.json` can be removed and `<TargetFrameworks>` reduced to `<TargetFramework>net8.0</TargetFramework>` if desired.

## Consequences

### Positive
- Spec compliance: `net8.0` is the explicit build target, matching AGENTS.md.
- Local gate stays green: `net10.0` is built and tested on this machine.
- CI deterministic: once the 8.0 SDK is added to the CI image, both `net8.0` and `net10.0` are exercised; no surprise build failures when the local 10.x SDK behaviour diverges.
- Forward-compatible: if Microsoft releases a future .NET LTS (net12, etc.), the multi-target layout makes the migration a one-line `<TargetFrameworks>net8.0;net12.0</TargetFrameworks>` change.

### Negative
- The Tenants module is shipped and tested against two runtimes. Any net10-only API accidentally used in shared code will fail `net8.0` on CI.
- `global.json` with `rollForward: latestMajor` means the **resolved** SDK locally is 10.0.203 even though the **pinned** version is 8.0.0. A developer who reads `global.json` and expects 8.0.0 will be surprised. The local-SDK constraint is documented in the **Install the 8.0 SDK** section below and in the O2 handoff in `orchestration.md`.

### Neutral
- No new dependencies; no new packages. The vendor DBTools_SQL stub and every other source file is unchanged.

## Local SDK constraint (today)

The macOS arm64 machine that authored this PR has:

- `dotnet --list-sdks` → `10.0.203`
- `dotnet --list-runtimes` → `Microsoft.AspNetCore.App 10.0.7`, `Microsoft.NETCore.App 8.0.26`, `Microsoft.NETCore.App 10.0.7`

`Microsoft.AspNetCore.App 8.0.0` (the exact version the testhost asks for when targeting `net8.0` with no rollForward) is **not** installed; the available `8.0.26` does not roll down to `8.0.0`, so the testhost aborts on `net8.0` with:

```
You must install or update .NET to run this application.
Framework: 'Microsoft.AspNetCore.App', version '8.0.0' (arm64)
```

This is the *expected* behaviour on this machine. CI will see the same multi-target layout and will need an 8.0.x SDK and an 8.0.0+ AspNetCore runtime in the image.

## Install the 8.0 SDK (one-liners)

To run the `net8.0` build and tests locally, install the 8.0 SDK on macOS arm64:

```bash
# Homebrew (Microsoft official tap):
brew tap isen-ng/dotnet-sdk-versions
brew install --cask dotnet-sdk@8
# or, official installer script:
curl -sSL https://dot.net/v1/dotnet-install.sh | bash /dev/stdin --channel 8.0 --install-dir "$HOME/.dotnet"
```

After install, `dotnet --list-sdks` should show both `8.0.x` and `10.0.203`. The `net8.0` build and tests will then run.

## Wave 1 status after this change

- `net10.0` build: **GREEN** (this machine, today).
- `net10.0` tests: **GREEN** (13/13 pass on this machine, today).
- `net8.0` build: **expected to fail** on this machine (no 8.0 SDK). This is the documented local-SDK constraint above; CI will run it once the 8.0 SDK is in the image.
- All other Wave 1 acceptance criteria (1.0a + 1.1) are unchanged.

## Related

- Open issue O2 in `orchestration.md` (Wave 1 handoff to Wave 2): the original `net10.0`-only build was a deliberate env-driven deviation; this ADR formalizes the multi-target reconciliation.
- Task 1.16 (`tasks.md`) — owner of the `docs/architecture/decisions/` directory. The directory is created by this fix-up; task 1.16 will own ADRs 0001 (modular monolith), 0002 (DBTools_SQL as only data access), 0003 (multi-tenant shared schema), and any other future ADRs.
