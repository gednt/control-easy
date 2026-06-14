# Requirements — Apartments module (Phase 1)

## UC1: List apartments

As a condominium attendant, I need to list apartments for my tenant so I can assign residents, vehicles, and visits to real apartment records.

## UC2: View apartment details

As a condominium attendant, I need to fetch a single apartment by id so I can verify block/unit before editing.

## UC3: Create apartment

As a tenant administrator, I need to create apartments with block and unit so the building structure is registered before linking entities.

## UC4: Update / deactivate apartment

As a tenant administrator, I need to update block/unit and soft-deactivate apartments so outdated units can be retired without deleting history.

## UC5: Demo seed consistency

As a demo user, I need demo apartments seeded with stable `DemoIds.ApartmentId()` values before residents, visits, and vehicles so cross-entity references resolve.

## UC6: Cross-entity apartment validation

As the system, when `ApartmentId` is supplied on resident, vehicle, or visit create, I need to assert the apartment exists for the current tenant so invalid references are rejected.

## UC7: Permission model

As a security administrator, I need `Apartments.Read` and `Apartments.Write` permissions available on attendant profiles (matching existing `Residents.Read` / `Residents.Write` patterns).
