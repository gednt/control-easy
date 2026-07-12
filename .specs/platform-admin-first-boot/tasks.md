# PlatformAdmin First-Boot Login — Tasks

## Task Dependency Graph

```json
{
  "waves": [
    { "wave": 1, "tasks": ["1"] },
    { "wave": 2, "tasks": ["2"] },
    { "wave": 3, "tasks": ["3"] }
  ]
}
```

---

- [x] **1. Extend PlatformAdminBootstrapService**
  - [x] Insert `AttendantProfiles` row with `platform:*` permissions after user creation.
  - [x] Keep idempotent skip when PlatformAdmin user already exists.
  - **Verification gate:** Manual or automated login with bootstrap credentials succeeds.

- [x] **2. Add tests**
  - [x] Unit test for bootstrap profile seeding logic, or integration test on non-demo factory.
  - **Verification gate:** `dotnet test` passes.

- [x] **3. Docker verification**
  - [x] Rebuild and recreate `api` / `web` containers.
  - **Verification gate:** Fresh volume bootstrap → login via API returns token.
