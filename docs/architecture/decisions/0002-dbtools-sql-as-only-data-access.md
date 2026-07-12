# 0002 — DBTools as the Sole Data-Access Layer

Date: 2026-06-13

## Status

Accepted

## Context

ControlEasy 5 (legacy WPF) uses `EntityFramework 6.4.4` with `MySql.Data` (hard-coded credentials in `App.config`). The modernization roadmap requires a data-access layer that:

1. Supports multiple database providers (MySQL today, PostgreSQL/SQL Server later) without code changes.
2. Prioritizes LINQ-based queries with lambda predicates.
3. Avoids tight coupling to any specific ORM or ADO.NET provider.
4. Provides an escape hatch for stored procedures or DB-specific features.

The options are:

1. **Entity Framework Core** — mature ORM, LINQ provider, migrations, multi-provider.
2. **Dapper** — micro-ORM, raw SQL, manual mapping.
3. **DBTools** ([NuGet package](https://www.nuget.org/packages/DBTools) 1.4.3, pinned in `src/Directory.Packages.props`) — multi-provider library with `LinqHelper<TModel>` / `Linq<TModel>` (lambda predicates, property selectors, JOINs, deferred `IQueryable`), plus `IAsyncSqlClient` for raw SQL escape hatch.

## Decision

We adopt **DBTools** (option 3) as the sole data-access layer, consumed as a versioned NuGet package (not vendored source).

All repository implementations use `TenantAwareLinqFactory` (which wraps `IAsyncSqlClient` with the `TenantFilterInterceptor`) for tenant-filtered queries. The factory produces per-scope `IAsyncSqlClient` instances with interceptors attached.

LINQ-first: queries are written via `IAsyncSqlClient.SelectAsync`, `InsertAsync`, `UpdateAsync`, `DeleteAsync` with parameterized WHERE clauses. Raw SQL via `IAsyncSqlClient` is reserved for stored procedures or DB-specific features not covered by the parameterized API.

**Forbidden:**
- `EntityFramework` / `Microsoft.EntityFrameworkCore` — not used in the new codebase.
- `MySql.Data` / `MySqlConnector` direct usage — all data access goes through DBTools_SQL.
- Hand-written SQL in repositories — use parameterized API methods; only escape to raw SQL for stored procedures.

The multi-provider configuration is in `appsettings.json` (`Db:Provider = "MySQL"` today). Switching to PostgreSQL or SQL Server requires only a configuration change and a provider-specific `DbProviderFactory` registration.

## Consequences

- **Pros:** Multi-provider abstraction; LINQ-first with escape hatch; no ORM overhead (change tracking, lazy loading); interceptor pipeline enables cross-cutting concerns (tenant filtering, audit, soft delete) without repository cooperation.
- **Cons:** DBTools is a smaller-community library than EF Core; some advanced query patterns may require raw SQL; no built-in migration system — schema changes are managed via SQL scripts in `docker/mysql/init/`.
- **Escape hatch:** `IAsyncSqlClient` exposes raw SQL execution for cases where the parameterized API is insufficient. This must be used sparingly and documented with a comment explaining why the LINQ API was insufficient.