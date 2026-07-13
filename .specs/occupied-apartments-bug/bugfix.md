# Bug Fix: Occupied Apartments Dashboard Count

## Overview

The dashboard "Occupied Apartments" stat tile shows incorrect values. When a user creates apartments with no residents assigned, the count is wrong (either 0 when it should show occupied apartments from demo data, or wrong values due to cross-tenant data leakage).

## Root Cause

The `occupiedApartments` query in `ReportReadRepository.cs:98-103` uses `SelectRawAsync` with a raw SQL JOIN:

```sql
SELECT DISTINCT a.Id FROM Apartments a INNER JOIN Residents r ON a.Id = r.ApartmentId WHERE r.Active = 1
```

The `TenantFilterInterceptor` injects `tenant_id = @paramN` into this query, producing:

```sql
SELECT DISTINCT a.Id FROM Apartments a INNER JOIN Residents r ON a.Id = r.ApartmentId WHERE tenant_id = @param0 AND r.Active = 1
```

Since **both** `Apartments` and `Residents` tables have a `tenant_id` column, MySQL throws error 1052 ("Column 'tenant_id' in where clause is ambiguous"). The exception is silently caught by `ExecuteReaderAsync` which returns an empty `DataTable`, resulting in `occupiedApartments = 0` regardless of actual data.

## Condition

1. Any dashboard stats request (`GET /api/v1/dashboard/stats`) triggers the query.
2. The `TenantFilterInterceptor` appends `tenant_id = @paramN` to the raw SQL.
3. The JOIN between `Apartments` and `Residents` creates an ambiguous column reference.
4. MySQL error 1052 is thrown and silently caught, returning empty result set.

## Examples

- Tenant with 5 apartments and 3 active residents: `occupiedApartments` shows **0** instead of **3**.
- Demo tenant with residents linked to apartments: `occupiedApartments` shows **0** instead of the correct count.
- Tenant with no residents: `occupiedApartments` shows **0** (coincidentally correct, but for the wrong reason).

## Fix Plan

1. Rewrite the `occupiedApartments` query to use **aliased** `tenant_id` references for both sides of the JOIN:
   ```sql
   SELECT DISTINCT a.Id FROM Apartments a INNER JOIN Residents r ON a.Id = r.ApartmentId WHERE a.tenant_id = @param0 AND r.tenant_id = @param1 AND r.Active = 1
   ```

2. Since the `TenantFilterInterceptor` cannot handle multi-table JOINs with ambiguous columns, the fix should either:
   - **Option A:** Rewrite using DBTools `SelectAsync` with separate queries (get apartment IDs from active residents, then count distinct).
   - **Option B:** Keep `SelectRawAsync` but manually add the aliased tenant filter to the raw SQL, and mark the interceptor to skip injection (set `__bypassTenantFilter` property or use a pattern the interceptor recognizes as already containing `tenant_id`).
   - **Option C (Recommended):** Rewrite the raw SQL to include explicit `a.tenant_id` and `r.tenant_id` filters, and ensure the interceptor detects `tenant_id` is already present in the SQL so it skips injection.

3. Add a unit test that verifies the `occupiedApartments` count is correct for a tenant with apartments and active residents.

4. Consider also fixing the `TenantFilterInterceptor` to detect JOIN scenarios and inject aliased predicates. This is a broader fix that prevents the same class of bug in future raw SQL queries.

## Affected Files

- `src/Modules/Reports/ControlEasyReborn.Modules.Reports.Infrastructure/Persistence/ReportReadRepository.cs` (lines 98-103)
- `src/BuildingBlocks/ControlEasyReborn.Infrastructure/MultiTenancy/TenantFilterInterceptor.cs` (interceptor logic)
- Tests for dashboard stats