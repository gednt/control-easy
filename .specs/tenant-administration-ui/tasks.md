# Tasks — Tenant Administration UI

## Task Dependency Graph

```json
{
  "waves": [
    { "wave": 1, "tasks": ["1", "2"] },
    { "wave": 2, "tasks": ["3", "4"] },
    { "wave": 3, "tasks": ["5", "6"] }
  ]
}
```

## Tasks

- [x] **1. List tenants API.** Add `ListAsync` to `ITenantRepository`, `ListTenantsHandler`, `GET /api/v1/tenants`, register handler in DI.
- [x] **2. Integration tests.** Add tests for list + create tenant as PlatformAdmin in integration test project.
- [x] **3. Tenants API service.** Angular `tenants-api.service.ts` with list, create, suspend, resume, createAdmin.
- [x] **4. Condominiums page.** Standalone page at `/platform/condominiums` with list table and register modal (optional first admin).
- [x] **5. Navigation and routing.** PlatformAdmin-only sidebar link; route in `app.routes.ts`; role check via `AuthService`.
- [x] **6. E2E test.** Playwright: PlatformAdmin registers a condominium and sees it in the list.
- [x] **7. Documentation.** Update `docs/getting-started.md` and `docs/demo-mode.md` with platform-admin condominium registration steps.

## Verification gates

- `dotnet build src/ControlEasyReborn.sln` succeeds
- `dotnet test tests/ControlEasyReborn.IntegrationTests` — tenant endpoint tests pass
- Docker rebuild: `docker compose -f docker/docker-compose.yml build api web && docker compose -f docker/docker-compose.yml up -d --force-recreate api web`
- PlatformAdmin can open `/platform/condominiums` and register a new condominium in the running stack
