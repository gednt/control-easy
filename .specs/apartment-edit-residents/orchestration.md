# Orchestration log -- Apartment edit residents

## Request

Extend the apartment edit modal to list active residents, add new residents, and deactivate existing ones.

## Classification

- **Type:** Feature extension
- **Domains:** Backend (residents API), Frontend (apartments page, residents API service)

## Agent routing

| Phase | Agent | Responsibility |
|-------|-------|---------------|
| 1 | Full-Stack Agent | Backend: apartmentId filter + UpdateResidentRequest.Active fix; Frontend: apartments edit modal residents management |

## Execution log

| Timestamp | Event |
|-----------|-------|
| 2026-06-15 | Backend: Added `apartmentId` filter to `IResidentRepository.ListAsync`, `ResidentRepository.ListAsync`, `ListResidentsHandler`, and `ResidentEndpoints` |
| 2026-06-15 | Backend: Added `Active` to `UpdateResidentRequest`, updated `UpdateResidentHandler` to call `Deactivate()` when `!request.Active` |
| 2026-06-15 | Frontend: Updated `residents-api.service.ts` -- `list()` now accepts `apartmentId`, `deactivate()` sends full payload |
| 2026-06-15 | Frontend: Fixed `residents.page.ts` `onEditResident()` to include `active: resident.active` and `onConfirmDeactivate()` to use full-payload deactivate |
| 2026-06-15 | Frontend: Expanded `apartments.page.ts` edit modal with residents list, inline add form (name/CPF/phone), and deactivate with confirmation dialog |
| 2026-06-15 | Backend build verified clean; Angular build verified clean |
| 2026-06-15 | Fix: `ResidentRepository.UpdateAsync` now uses parameterized `Id = @param7` WHERE clause so tenant filter interceptor applies on deactivate updates |
| 2026-06-15 | Fix: `apartments.page.ts` resident deactivate confirm uses `deactivatingResidentId` spinner and surfaces API errors via `deactivateResidentError` |
| 2026-06-15 | Tests: 17/17 resident unit tests pass; Docker api+web rebuilt and restarted |

## Status

| Phase | Agent | Status |
|-------|-------|--------|
| 1 | Full-Stack Agent | QA complete — deactivate route fixed end-to-end |