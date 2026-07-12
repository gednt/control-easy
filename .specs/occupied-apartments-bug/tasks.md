# Bug Fix: Occupied Apartments Dashboard Count

## Tasks

- [ ] **1.** Fix the `occupiedApartments` raw SQL query in `ReportReadRepository.cs` to include aliased tenant filters, bypassing the interceptor's ambiguous injection
  - Add `a.tenant_id = @paramN AND r.tenant_id = @paramN+1` directly in the raw SQL
  - Ensure the SQL contains `tenant_id` so the interceptor skips injection (its `Contains("tenant_id")` check)
  - Verification gate: `GET /api/v1/dashboard/stats` returns correct `occupiedApartments` count

- [ ] **2.** Add unit/integration test for `occupiedApartments` counting
  - Test tenant with apartments and active residents → correct count
  - Test tenant with apartments but no residents → 0
  - Test tenant with apartments and inactive residents → 0
  - Verification gate: tests pass

- [ ] **3.** Rebuild Docker stack and verify
  - Verification gate: `docker compose -f docker/docker-compose.yml build api && docker compose -f docker/docker-compose.yml up -d --force-recreate api`

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