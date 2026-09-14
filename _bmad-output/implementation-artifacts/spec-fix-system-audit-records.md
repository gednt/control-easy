---
title: 'Restore system-wide audit records'
type: 'bugfix'
created: '2026-09-13'
status: 'done'
review_loop_iteration: 0
baseline_commit: '489ffa317afb6ef93e01fa00c50e98095ead88b7'
context:
  - '{project-root}/AGENTS.md'
---

<frozen-after-approval reason="human-owned intent — do not modify unless human renegotiates">

## Intent

**Problem:** The Administration page's System-Wide Security Audit reports no records even when audit events exist for the current condominium. Its list query combines ordering with filter text, so the mandatory tenant interceptor appends its predicate after `ORDER BY`, producing invalid SQL. The Angular error handler then renders the normal empty state instead of identifying the failed load.

**Approach:** Make tenant-scoped audit-list retrieval execute safely with the global tenant filter, preserve newest-first ordering in application code, and distinguish a failed retrieval from a genuinely empty audit log. The repaired view must return the current tenant's existing `AuditLog` events and retain all supported filters.

## Boundaries & Constraints

**Always:** Preserve the `ITenantAwareLinqFactory` path for every AuditLog read; ensure tenant isolation remains enforced by `tenant_id`; retain category, severity, entity type, action, date-range, text-search, pagination, and newest-first behavior; use ProblemDetails for API failures; test inside Docker only.

**Ask First:** Expanding the System-Wide Security Audit to merge or backfill historical `ConsentAuditLog` gatehouse records is a separate product/data-migration decision. This repair guarantees records already written to `AuditLog`; it does not reinterpret the dedicated entry-log ledger as system-audit data.

**Never:** Bypass tenant filtering, use EF Core or MediatR, weaken authorization, introduce raw ad-hoc SQL, or modify/delete audit data.

## I/O & Edge-Case Matrix

| Scenario | Input / State | Expected Output / Behavior | Error Handling |
|----------|---------------|----------------------------|----------------|
| Existing audit events | Authenticated tenant user; matching `AuditLog` rows | `GET /api/v1/administration/audit-logs` returns the current tenant's events, newest first, and the Administration table renders them | N/A |
| Filtered audit events | Category, severity, date, text, and paging parameters | Only matching current-tenant events are returned; paging is applied after descending sort | N/A |
| Other-tenant events | Rows whose `tenant_id` differs from request tenant | No foreign-tenant rows are exposed | N/A |
| Retrieval failure | API returns a non-success response | The page keeps no stale results and presents an audit-load error, rather than claiming no records exist | Preserve the API error as a user-visible message |

</frozen-after-approval>

## Code Map

- `src/Modules/Administration/ControlEasyReborn.Modules.Administration.Infrastructure/Persistence/AuditLogRepository.cs` -- constructs the list and entity-history queries; currently embeds ordering in the DBTools predicate string.
- `src/BuildingBlocks/ControlEasyReborn.Infrastructure/MultiTenancy/TenantFilterInterceptor.cs` -- appends the mandatory `tenant_id` predicate to DBTools select operations.
- `src/Modules/Administration/ControlEasyReborn.Modules.Administration.Api/Endpoints/AdministrationEndpoints.cs` -- tenant-resolved system-audit list endpoint.
- `src/Web/ControlEasyReborn.Web/src/app/features/administration/administration.page.ts` -- invokes the endpoint and currently suppresses list failures.
- `tests/ControlEasyReborn.UnitTests/Modules/Administration/AuditLogHandlerTests.cs` -- existing handler coverage to extend for list behavior.
- `tests/ControlEasyReborn.UnitTests/BuildingBlocks/TenantFilterInterceptorTests.cs` -- tenant SQL-injection behavior, if present; otherwise add focused coverage in the appropriate tenant-filter test project.

## Tasks & Acceptance

**Execution:**

- [x] `src/Modules/Administration/ControlEasyReborn.Modules.Administration.Infrastructure/Persistence/AuditLogRepository.cs` -- pass only predicates to DBTools, sort mapped audit events in memory before pagination, and apply the same tenant-safe ordering correction to entity history -- prevents the tenant interceptor from corrupting generated SQL.
- [x] `src/Web/ControlEasyReborn.Web/src/app/features/administration/administration.page.ts` and its spec -- represent a failed audit request separately from an empty list and show a concise retryable error -- prevents unavailable records being misrepresented as absent.
- [x] `tests/ControlEasyReborn.UnitTests/Modules/Administration/AuditLogHandlerTests.cs` plus the relevant tenant-filter/repository tests -- cover query ordering with the tenant predicate, newest-first/pagination/filter mapping, and the non-leakage contract -- protects the regression path.
- [x] `src/Web/ControlEasyReborn.Web/src/app/features/administration/administration.page.spec.ts` -- assert audit-load failure state and recovery on a subsequent successful request -- covers the UI's factual empty/error distinction.

**Acceptance Criteria:**

- Given `AuditLog` contains matching records for the authenticated tenant, when the System-Wide Security Audit loads, then it shows those records newest first instead of the empty-state message.
- Given audit events for another tenant, when a tenant-scoped audit request is made, then none of the other tenant's records appear.
- Given a category, severity, text, date, or pagination filter, when the audit list loads, then the selected filter behavior is preserved after the query correction.
- Given the audit-list request fails, when the page completes the request, then it shows an error/retry affordance and does not label the failure “No audit records found.”

## Spec Change Log

## Design Notes

The generic System-Wide Security Audit and the gatehouse consent ledger are distinct data sets. The current page calls the former (`/administration/audit-logs`), whose rows are written by `IAuditLogWriter`; `ConsentAuditLog` remains the specialized append-only entry ledger exposed by `/entry-log`. Conflating them would require an explicit product decision about schema, event normalization, and historical data migration.

The underlying generated query must have the form below so the interceptor can safely append the second predicate:

```sql
SELECT * FROM AuditLog
WHERE TenantId = @param0 AND tenant_id = @param1
```

Sorting must happen after mapping, matching the established `ConsentAuditLogRepository` approach, instead of putting `ORDER BY` inside a DBTools `whereClause`.

## Verification

**Commands:**

- `docker compose -f docker/docker-compose.yml build api web` -- expected: both images build without warnings/errors.
- `docker compose -f docker/docker-compose.yml up -d --force-recreate api web` -- expected: API and SPA start healthy.
- `docker compose -f docker/docker-compose.yml exec -T api dotnet test /workspace/tests/ControlEasyReborn.UnitTests` -- expected: all unit tests pass.
- `docker compose -f docker/docker-compose.yml exec -T api dotnet test /workspace/tests/ControlEasyReborn.IntegrationTests` -- expected: all integration tests pass.
- `docker compose -f docker/docker-compose.yml exec -T api dotnet test /workspace/tests/ControlEasyReborn.ArchitectureTests` -- expected: all architecture tests pass.
- `docker compose -f docker/docker-compose.yml exec -T web npm test -- --no-watch --browsers=ChromeHeadless` -- expected: Angular unit tests pass, including AdministrationPage audit error/recovery coverage.

## Suggested Review Order

**Audit retrieval correctness**

- Build valid tenant-filterable predicates, then apply stable sorting and paging.
  [`AuditLogRepository.cs:147`](../../src/Modules/Administration/ControlEasyReborn.Modules.Administration.Infrastructure/Persistence/AuditLogRepository.cs#L147)

- Ensure only the latest audit request can update the current filtered view.
  [`administration.page.ts:708`](../../src/Web/ControlEasyReborn.Web/src/app/features/administration/administration.page.ts#L708)

**Truthful audit feedback**

- Render loading, failure, and genuine-empty states as distinct user outcomes.
  [`administration.page.ts:358`](../../src/Web/ControlEasyReborn.Web/src/app/features/administration/administration.page.ts#L358)

- Expose only safe ProblemDetails titles, never server diagnostic details.
  [`administration.page.ts:761`](../../src/Web/ControlEasyReborn.Web/src/app/features/administration/administration.page.ts#L761)

**Regression coverage**

- Verify generated predicates, parameter order, pagination, and stable entity-history ordering.
  [`AuditLogRepositoryTests.cs:27`](../../tests/ControlEasyReborn.UnitTests/Modules/Administration/AuditLogRepositoryTests.cs#L27)

- Cover error recovery, stale responses, and safe API-error presentation in the audit UI.
  [`administration.page.spec.ts:135`](../../src/Web/ControlEasyReborn.Web/src/app/features/administration/administration.page.spec.ts#L135)
