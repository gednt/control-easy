# 0003 — Multi-Tenant Shared Schema

Date: 2026-06-13

## Status

Accepted

## Context

ControlEasy Reborn must support multiple tenants (condominiums) on a single shared deployment. The options for data isolation are:

1. **Database per tenant** — separate MySQL databases per condominium.
2. **Schema per tenant** — separate MySQL schemas per condominium in one server.
3. **Shared schema with discriminator** — single schema, `tenant_id` column on every business table.

## Decision

We adopt **shared schema with `tenant_id` discriminator** (option 3).

A `TenantFilterInterceptor : IQueryInterceptor` (DBTools_SQL) injects `WHERE tenant_id = @ctx_tenant` into every query built through `TenantAwareLinqFactory`. The interceptor is the single point where the global tenant filter is applied; repositories never need to remember to add it themselves.

The `Tenants` table is a `Platform*` root aggregate exempt from the filter (bypass mode).

## Consequences

- **Pros:** Single schema to manage, simpler migrations, efficient connection pooling, zero cross-tenant leakage when the interceptor is correct.
- **Cons:** All queries include an extra predicate; careful index strategy needed on `tenant_id`; backup/restore per tenant requires `mysqldump --where` filtering; noisy-neighbor risk on shared I/O.
- **WPF compatibility:** `_legacy` views hard-code `WHERE tenant_id = '00000000-...'` so the WPF app reads/writes through the views with no code change. These views are removed in task 4.4.
- **Future:** If a tenant grows large enough to warrant isolation, the same `tenant_id` column enables sharding or migration to a per-tenant database with minimal code changes (only the connection string routing changes).