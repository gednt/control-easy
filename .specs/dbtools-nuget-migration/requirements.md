# Requirements — DBTools NuGet Migration

> Spec folder: `.specs/dbtools-nuget-migration/`
> Goal: Replace the vendored `src/lib/DBTools_SQL/` copy with the official [DBTools 1.4.3 NuGet package](https://www.nuget.org/packages/DBTools) from nuget.org.

## User Stories

- **UC-1:** As a *developer*, I want DBTools consumed as a versioned NuGet package so dependency updates are managed through CPM instead of vendored source copies.
- **UC-2:** As a *DevOps engineer*, I want the API Docker build to restore DBTools from NuGet so the Dockerfile no longer copies `src/lib/DBTools_SQL/`.
- **UC-3:** As a *maintainer*, I want all existing repositories, interceptors, and integration tests to pass unchanged after the swap so tenant isolation and LINQ queries remain correct.

## Acceptance Criteria

1. `Directory.Packages.props` pins `DBTools` at `1.4.3`; `MySqlConnector` remains pinned for MySQL provider support.
2. `ControlEasyReborn.Infrastructure.csproj` references the NuGet package — no `<ProjectReference>` to `src/lib/DBTools_SQL/`.
3. `src/lib/DBTools_SQL/` and the DBTools solution folder are removed from the repo.
4. `docker/api.Dockerfile` no longer copies the vendored library path.
5. `dotnet build` and `dotnet test` pass (unit + integration + architecture).
6. `docker compose build api && docker compose up -d` brings the API healthy.
7. ADR `0002-dbtools-sql-as-only-data-access.md` and `AGENTS.md` reference the NuGet package, not vendored source.

## Out of Scope

- Upgrading beyond 1.4.3 unless required for compile compatibility
- Switching database provider (MySQL remains default)
- Refactoring repository patterns or `TenantFilterInterceptor` behavior
