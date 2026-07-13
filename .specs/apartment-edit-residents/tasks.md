# Tasks -- Apartment edit: manage residents

- [x] 1. Backend: Add `apartmentId` filter to `IResidentRepository.ListAsync`, `ResidentRepository.ListAsync`, `ListResidentsHandler`, and `ResidentEndpoints`
- [x] 2. Backend: Add `Active` to `UpdateResidentRequest`, update `UpdateResidentHandler` to call `Deactivate()` when `!request.Active`
- [x] 3. Frontend: Update `residents-api.service.ts` -- add `apartmentId` to `list()`, fix `deactivate()` to send full payload
- [x] 4. Frontend: Fix `residents.page.ts` `onEditResident()` to include `active` in update payload
- [x] 5. Frontend: Expand `apartments.page.ts` edit modal -- residents list, inline add form, deactivate with confirm
- [x] 6. QA: Integration tests for apartment filter and resident deactivate
- [x] 7. QA: Playwright E2E for apartment residents flow