# Orchestration — PlatformAdmin First-Boot Login Fix

## Request

User asked to **fix the PlatformAdmin first-boot login gap** before implementing the getting-started documentation (`.specs/getting-started/`).

## Classification

**Bug fix** — bootstrap creates a user that cannot complete `POST /api/v1/security/auth/login`.

## Routing

| Phase | Agent | Scope |
|---|---|---|
| 1 | **Backend Agent** | Extend `PlatformAdminBootstrapService` to seed an `AttendantProfile`; add unit/integration tests; rebuild Docker stack |
| 2 | **Documentation Agent** | Implement `docs/getting-started.md` (remove workaround/limitation notes from doc plan) |

## Root cause

`PlatformAdminBootstrapService` inserts a `Users` row only. `LoginHandler` requires at least one active `AttendantProfile` (`ListByUserAsync` → `FirstOrDefault(p => p.Active)`). Login fails with `"No active attendant profile found for user."`

Demo mode avoids this: `DemoSeederService.SeedAttendantProfilesAsync` creates a platform profile with `platform:*` permissions.

## Fix approach (Backend Agent)

1. After user insert in `PlatformAdminBootstrapService`, insert an `AttendantProfiles` row:
   - Same `TenantId` / `tenant_id` as default tenant (`00000000-0000-0000-0000-000000000001`)
   - `Permissions = "platform:*"` (matches demo seeder)
   - `ShiftId` / `GatehouseId` = null (no default shift/gatehouse on normal first boot)
   - Idempotent: skip if profile already exists for the bootstrapped user
2. Add unit test covering bootstrap profile creation (mock `IAsyncSqlClient` or extract testable helper).
3. Add integration test: normal (non-demo) factory → login with credentials from bootstrap logs (or seed + login flow).
4. Run `dotnet test` and Docker rebuild per AGENTS.md.

## Out of scope (follow-up)

- `CreateTenantAdminHandler` / `TenantAdminRepository` also omit `AttendantProfile` — separate bug if tenant admins cannot log in via UI.
- LoginHandler special-casing PlatformAdmin without profile — rejected; profile seeding is the intended model per design.md.

## Dependencies for Documentation Agent (Phase 2)

- Phase 2 starts only after Phase 1 verification gates pass.
- Update `.specs/getting-started/docplan.md` to remove the "login may fail" limitation.
- Proceed with user-confirmed language (English default).

## Status

| Phase | Agent | Status |
|---|---|---|
| 1 | Backend Agent | Complete — `AttendantProfiles` seeded with `platform:*`; login verified HTTP 200 |
| 2 | Documentation Agent | Complete — `docs/getting-started.md` + README/demo cross-links |
