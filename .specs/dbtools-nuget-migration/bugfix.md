# Bugfix — DBTools NuGet 1.4.3 `SelectAsync` rejects backtick-quoted field names

**Spec:** `.specs/dbtools-nuget-migration/`
**Raised:** 2026-07-12 during Stream 0.5 integration-test re-run
**Root cause:** API behavior change between vendored `src/lib/DBTools_SQL/` and NuGet `DBTools` 1.4.3

## Overview

The Phase 7 DBTools NuGet migration compiled and passed unit tests, but the integration test suite exposed a runtime regression. The NuGet package's `AsyncSqlClient.SelectAsync` validates the `fields` parameter and throws `System.ArgumentException: Invalid field names.` when the field list contains backtick-quoted identifiers such as `` `Key` ``. The vendored library accepted them.

This breaks any repository that uses `` `Key` `` (a MySQL reserved word) in a `SelectAsync` call.

## Condition

- Code uses `db.SelectAsync(fields: "... `Key` ...", ...)`.
- Runtime executes against the NuGet `DBTools` 1.4.3 package.
- `SelectAsync` internal field-name validator rejects backticks in the comma-separated field list.

## Affected code

```csharp
// src/Modules/Administration/ControlEasyReborn.Modules.Administration.Infrastructure/Persistence/ConfigurationRepository.cs
fields: "Id, TenantId, `Key`, Value, Description, CreatedAtUtc, UpdatedAtUtc"
```

Three call sites in `ConfigurationRepository` use the backtick-quoted field list. The `whereClause` parameter (`` `Key` = @param1``) is not validated in the same way and remains valid.

## Fix plan

1. Remove backticks from the `fields` parameter in all three `SelectAsync` calls in `ConfigurationRepository.cs`.
2. Leave the `whereClause` backticks intact so MySQL still receives a valid quoted identifier.
3. Re-run integration tests (`AdministrationEndpointTests` and full suite) to confirm green.
4. Update `07-VERIFICATION.md` Notes with the re-run date and result.

## Verification

- `dotnet test tests/ControlEasyReborn.IntegrationTests --no-build` passes.
- `docker compose -f docker/docker-compose.yml build api && docker compose -f docker/docker-compose.yml up -d --force-recreate api db` produces a healthy API container.

## Traceability

- Maps to PLAT-01 (Phase 7 — DBTools NuGet Migration).
- Maps to FOUND-07 (integration tests with Testcontainers.MySql).
- Closes the open concern in `.planning/phases/07-dbtools-nuget-migration-inserted/07-VERIFICATION.md` Notes section.
