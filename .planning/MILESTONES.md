# Milestones

## v1.0 Reborn MVP (Shipped: 2026-07-13)

**Phases completed:** 11 phases, 11 plans, 0 tasks

**Delivered:** A tenant-isolated ASP.NET Core and Angular access-control platform with core condominium modules, canonical Docker runtime, hardened design system, demo environment, and continuous engineering gates.

**Key accomplishments:**

- Shipped the modular monolith and tenant-aware Residents, Visits, Vehicles, ServiceProviders, Apartments, Security, Administration, and Reports slices.
- Migrated persistence to the official DBTools 1.4.3 NuGet package and added a DBTools-backed runtime health check.
- Hardened the Angular design system with canonical tokens, reusable `ce-*` components, accessibility coverage, and visual regression snapshots.
- Added deterministic demo mode, first-boot administration, and tenant-switching flows.
- Added GitHub Actions, generated OpenAPI artifacts with drift enforcement, tenant-isolation guards, and token contract tests.

**Verification:** 79 unit, 55 integration, 6 architecture, and 203 Angular tests passed; Docker API/web rebuilt and `/health` returned `Healthy`.

**Known debt:** Generated OpenAPI artifacts are drift-checked but handwritten Angular services do not consume them yet.

**What's next:** v1.1 UI parity, dashboard live data, and vehicle editing.

---
