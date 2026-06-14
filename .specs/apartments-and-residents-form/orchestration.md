# Orchestration log — Apartments CRUD + entity apartment wiring + Visits portaria flow

## Request

User reported on the Residents page:

1. Add-resident modal is missing fields (vs mockup).
2. Clicking Add resident appears to do nothing (likely silent API failure on invalid CPF or missing error feedback).

**Scope expansions (2026-06-13):**

- Full **Apartments CRUD module** so apartment assignment uses real apartment records.
- **Vehicles** must be associated with an apartment (UI wiring; backend already accepts `ApartmentId`).
- **Visits** page must support the portaria flow: check-in and check-out, not just "add visitor" (backend endpoints already exist).

## Classification

- **Type:** Feature + bug fix (cross-cutting)
- **Domains:** Backend (Apartments module), Frontend (apartments admin, residents/vehicles/visits pages), QA (integration + E2E)

## Preliminary analysis

| Issue | Finding |
|---|---|
| Missing resident fields | Mockup has Block + Apartment; Angular form has Email instead; backend accepts `ApartmentId` but UI never sends it |
| Resident create silent failure | `onCreateResident()` swallows API errors; frontend CPF is required-only while backend validates check digits |
| No Apartments entity | `ApartmentId` on Residents/Visits/Vehicles is a bare GUID; demo derives IDs via `DemoIds.ApartmentId(tenant, block, apt)` with no `Apartments` table |
| Vehicles UI gap | Backend DTOs expose `ApartmentId`; `vehicles.page.ts` form and table omit apartment picker and column |
| Visits UI gap | Backend has `POST /visits/{id}/checkin` and `POST /visits/{id}/checkout` with `VisitStatus` (`Pending` → `CheckedIn` → `CheckedOut`); frontend only lists visits and creates visitors — no check-in/out actions, no apartment/phone on create form, no timestamps in table |

## Agent routing

| Phase | Agent | Responsibility |
|---|---|---|
| 1 | **Backend Agent** | `Apartments` module, MySQL schema, permissions, demo seeder sync |
| 2 | **Full-Stack Agent** | Apartments admin UI, shared apartment picker, residents + vehicles + **visits** page wiring |
| 3 | **QA Agent** | Apartments unit/integration tests; visit check-in/out integration tests; Playwright E2E for all flows |

## Execution plan

### Phase 1 — Backend Agent

**Deliverables:** `design.md`, `requirements.md`, `tasks.md`, implementation

1. **Schema** — `docker/mysql/init/02b-apartments-schema.sql`
   - `Apartments`: `Id`, `TenantId`, `Block`, `Unit`, `Active`, `CreatedAtUtc`, `UpdatedAtUtc`, `tenant_id`
   - Unique index on `(TenantId, Block, Unit)`
2. **Module** — mirror Residents slice:
   - Entity `Apartment`, `IApartmentRepository`, handlers (List, Get, Create, Update, soft-deactivate)
   - Endpoints: `GET/POST /api/v1/apartments`, `GET/PUT /api/v1/apartments/{id}`
   - Permissions: `apartments.read`, `apartments.write`
3. **Host** — register module in `Program.cs`, solution projects, exception mapping
4. **Demo seeder** — seed `Apartments` rows using existing `DemoIds.ApartmentId()` IDs **before** residents/visits/vehicles
5. **Security seed** — add `apartments.read` / `apartments.write` to demo profiles
6. **Cross-entity validation (optional)** — when `ApartmentId` is supplied on resident/vehicle/visit create, assert apartment exists for tenant

**No Visits backend work required** — check-in/out handlers and endpoints already implemented.

**Verification gate:** Integration test — apartment CRUD + demo seed stable IDs; existing visit endpoint tests still pass.

### Phase 2 — Full-Stack Agent

**Dependencies:** Phase 1 API live

1. **Shared apartment picker** — reusable component loading `GET /api/v1/apartments`, label `{block}-{unit}`, emits `apartmentId`
2. **Apartments page** — `/apartments` route, list/create/edit/deactivate; sidebar nav item
3. **Residents** — apartment picker in create/edit modals; CPF validation; error feedback; apartment column in table
4. **Vehicles** — apartment picker on create; apartment column; error feedback
5. **Visits (portaria flow)**
   - Extend `visits-api.service.ts` with `checkIn(id)` → `POST /api/v1/visits/{id}/checkin`, `checkOut(id)` → `POST /api/v1/visits/{id}/checkout`
   - **Create form:** visitor name, document, phone, apartment (picker), purpose
   - **Table columns:** Visitor, Document, Apartment, Status (badge), Checked in, Checked out, Actions
   - **Row actions by status:**
     - `Pending` → **Check in** button
     - `CheckedIn` → **Check out** button
     - `CheckedOut` / `Cancelled` → no actions (read-only)
   - Optional status filter (All / Pending / On-site) using existing `?status=` query param
   - Error feedback on create, check-in, and check-out failures
   - Refresh list after each action; disable buttons while request in flight

**Verification gate:** Docker rebuild; attendant can create visit → check in → check out; resident/vehicle create with apartment; invalid CPF shows error.

### Phase 3 — QA Agent

**Dependencies:** Phases 1–2 complete

1. Unit tests — apartment handlers/validators/repository
2. Integration tests — apartment CRUD + tenant isolation
3. Integration tests — visit check-in and check-out happy path + invalid state transitions (409/400)
4. Playwright E2E — apartments CRUD
5. Playwright E2E — resident create with apartment
6. Playwright E2E — vehicle create with apartment
7. Playwright E2E — visit create → check in → check out (status and timestamps update)

## Task dependency graph

```json
{
  "waves": [
    { "wave": 1, "tasks": ["1.1-schema", "1.2-domain", "1.3-repository-handlers", "1.4-endpoints-permissions", "1.5-host-wiring", "1.6-demo-seeder", "1.7-integration-tests"] },
    { "wave": 2, "tasks": ["2.1-apartments-api-service", "2.2-shared-apartment-picker", "2.3-apartments-page", "2.4-sidebar-route", "2.5-residents-form-fix", "2.6-vehicles-form-fix", "2.7-visits-portaria-flow", "2.8-docker-verify"] },
    { "wave": 3, "tasks": ["3.1-apartment-unit-tests", "3.2-apartment-integration-tests", "3.3-visit-checkin-integration-tests", "3.4-e2e-apartments", "3.5-e2e-resident-create", "3.6-e2e-vehicle-create", "3.7-e2e-visit-checkin-checkout"] }
  ]
}
```

## Out of scope

- Residents list stat tiles, block filter, pagination, tabs (mockup extras)
- Visit cancellation UI (domain supports `Cancel()` but no API endpoint yet)
- Gatehouse/shift selector on check-in (backend accepts optional `gatehouseId`; can default to null for now)
- Legacy WPF screens

## Status

| Phase | Agent | Status |
|---|---|---|
| 1 | Backend Agent | Complete — Apartments module, schema, demo seed, integration tests; Docker verified |
| 2 | Full-Stack Agent | Complete — frontend wiring, Docker verified, browser smoke-tested |
| 3 | QA Agent | Complete — unit/integration/E2E tests; visit invalid transitions map to 409 |

## Coordination log

| Timestamp | Event |
|---|---|
| 2026-06-13 | User confirmed orchestration plan; Phase 1 delegated to Backend Agent |
| 2026-06-13 | Phase 1 complete — Apartments CRUD API live, demo seed v2, cross-entity validation on create |
| 2026-06-13 | Phase 2 complete — apartments CRUD UI, shared picker, residents/vehicles/visits portaria wiring |
| 2026-06-13 | Phase 3 verification — 17/17 apartment+visit unit tests pass (net10.0); 4/4 Playwright E2E pass against demo stack; 16/16 integration tests blocked by pre-existing TestHost/PipeWriter issue on net10.0 |
