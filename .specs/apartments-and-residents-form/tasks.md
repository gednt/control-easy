# Tasks — Apartments module + frontend wiring

## Phase 1 (Backend)

- [x] 1. Schema — `02b-apartments-schema.sql` + backfill marker
- [x] 2. Apartments module — Domain, Application, Infrastructure, Api (mirror Residents)
- [x] 3. Permissions — `Apartments.Read` / `Apartments.Write` in Security + demo profiles
- [x] 4. Host wiring — Program.cs, solution projects, GlobalExceptionHandler
- [x] 5. Demo seeder — seed apartments before residents/visits/vehicles; bump SeedVersion to 2
- [x] 6. Cross-entity validation — resident/vehicle/visit create checks apartment exists
- [x] 7. Integration tests — apartment CRUD + demo stable IDs; visit tests pass
- [x] 8. Verification gate — build, test, docker demo rebuild

## Phase 2 (Frontend)

- [x] 9. Apartments API service — `apartments-api.service.ts`
- [x] 10. Shared apartment picker — reusable component, `{block}-{unit}` label
- [x] 11. Apartments page — `/apartments` route, CRUD + sidebar nav
- [x] 12. Residents wiring — apartment picker, CPF validation, error feedback, apartment column
- [x] 13. Vehicles wiring — apartment picker on create, apartment column, error feedback
- [x] 14. Visits portaria flow — check-in/out API, form fields, table columns, status filter, actions
- [x] 15. Verification gate — docker demo rebuild + browser verify all flows

## Phase 3 (QA)

- [x] 16. Unit tests — apartment handlers, validators, repository
- [x] 17. Integration tests — apartment CRUD + tenant isolation (Phase 1 baseline retained)
- [x] 18. Integration tests — visit check-in/out happy path + invalid transitions (409/400)
- [x] 19. Playwright E2E — apartments CRUD
- [x] 20. Playwright E2E — resident create with apartment
- [x] 21. Playwright E2E — vehicle create with apartment
- [x] 22. Playwright E2E — visit create → check in → check out
- [x] 23. Verification gate — unit/integration filter + Playwright against demo stack

## Task Dependency Graph

```json
{
  "waves": [
    { "wave": 1, "tasks": ["1"] },
    { "wave": 2, "tasks": ["2", "3"] },
    { "wave": 3, "tasks": ["4", "5", "6"] },
    { "wave": 4, "tasks": ["7", "8"] },
    { "wave": 5, "tasks": ["9", "10"] },
    { "wave": 6, "tasks": ["11", "12", "13", "14"] },
    { "wave": 7, "tasks": ["15"] },
    { "wave": 8, "tasks": ["16", "17", "18"] },
    { "wave": 9, "tasks": ["19", "20", "21", "22"] },
    { "wave": 10, "tasks": ["23"] }
  ]
}
```
