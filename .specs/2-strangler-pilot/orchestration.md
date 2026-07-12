# Orchestration Log -- Phase 2: Strangler Pilot

## Status: IN PROGRESS (Wave 2 complete, Wave 3 pending)

## Prerequisites (completed before Phase 2 start)
- Phase 1: All 17 tasks verified green
- Fix: IntegrationTests net8.0 build error resolved (added `Program.Public.cs` partial class)
- Unit tests: 42/42 passing on net10.0 (was 26, +16 from Security module)
- Architecture tests: 5/5 passing
- Build: 0 errors, 9 warnings (all in vendor DBTools_SQL dependency)

## Wave 1 -- Backend: Security module + Attendant profile API (COMPLETE)
- [x] **2.1** Security module (Backend Agent) -- DONE
  - Created 4 Security module projects (Domain, Application, Infrastructure, Api)
  - Entities: User, AttendantProfile, Shift, Gatehouse (all with TenantId)
  - Repositories: All use ITenantAwareLinqFactory except UserRepository (platform entity, bypasses filter)
  - Handlers: Login, Refresh, CreateUser, AttendantProfile CRUD, Shift/Gatehouse CRUD, TenantLookup, TenantSwitch
  - Auth: RequirePermissionAttribute + handler, Permissions static class with canonical keys
  - JWT: JwtTokenService with all required claims (sub, tenant_id, profile_id, roles, permissions)
  - BCrypt: IPasswordHasher + PasswordHasher using BCrypt.Net-Next
  - DB: 05-security-schema.sql (AttendantProfiles, Shifts, Gatehouses, RefreshTokens)
  - Updated: 03-tenant-backfill.sql, 04-tenant-views.sql, PlatformAdminBootstrapService (BCrypt)
  - Fix: Added TenantId to User entity and updated UserRepository field mappings
  - 16 unit tests + integration tests for cross-tenant access denial
- [x] **2.7a** Attendant profile API surface (Backend Agent) -- DONE
  - Covered by Task 2.1 (Security module includes all attendant profile endpoints)
  - OpenAPI schema includes AttendantProfile, Shift, Gatehouse TypeScript types

## Wave 2 -- Frontend + Strangler wiring (COMPLETE)
- [x] **2.2** Angular Residents admin + login page (Frontend Agent) -- DONE
  - Login page with tenant picker, reactive forms, real API calls
  - AuthService rewritten with real JWT flow, token refresh, tenant switch, permissions
  - Auth interceptor updated to exclude login/tenant-lookup endpoints
  - Error interceptor with 401 refresh and 403 access-denied handling
  - Residents page enhanced with edit modal, deactivate action, debounced search
  - SecurityApiService with TypeScript interfaces
  - Angular build succeeds (login-page + residents-page chunks visible)
- [x] **2.3** WPF-in-parallel feature flag plumbing (Backend Agent) -- DONE
  - FeatureFlags.cs added to SharedKernel
  - GET /api/v1/features endpoint using IFeatureManagerSnapshot
  - Residents.UseWeb flag defaults to false in appsettings.json
  - Registered in Program.cs

## Wave 3 -- Pilot cutover (PENDING)
- [ ] **2.4** WPF/web cross-verify smoke test (QA Agent)
- [ ] **2.5** 7-day pilot flip on one condominium (DevOps Agent) -- requires running deployment
- [ ] **2.6** Legacy-mapping doc update (Documentation Agent)

## Agent Assignments
| Task | Agent | Phase | Status |
|------|-------|-------|--------|
| 2.1  | Backend | Wave 1 | COMPLETE |
| 2.7a | Backend | Wave 1 | COMPLETE (merged with 2.1) |
| 2.2  | Frontend | Wave 2 | COMPLETE |
| 2.3  | Backend | Wave 2 | COMPLETE |
| 2.4  | QA | Wave 3 | Pending |
| 2.5  | DevOps | Wave 3 | Pending |
| 2.6  | Documentation | Wave 3 | Pending |

## Issues Encountered
1. **IntegrationTests net8.0 build error**: `Program` class accessibility. Fixed by adding `Program.Public.cs` with `public partial class Program { }` in the Host project.
2. **User entity missing TenantId**: C# domain model didn't expose `TenantId` even though DB schema had it. Fixed by adding `Guid TenantId` property and updating UserRepository field mappings.
3. **Permissions.cs location**: Was in Application root instead of `Permissions/` subdirectory. Moved to correct location.

## Verification Status
- `dotnet build src/ControlEasyReborn.sln`: 0 errors
- `dotnet test` (unit): 42/42 passed
- `dotnet test` (architecture): 5/5 passed
- `npm run build` (Angular): SUCCESS
- No EntityFramework or MySql.Data references in Security module
- All per-tenant entities have non-nullable Guid TenantId