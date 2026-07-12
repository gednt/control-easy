# Requirements -- Apartment edit: manage residents

## UC1: View residents in apartment edit modal

As a condominium attendant, when editing an apartment, I need to see a list of active residents assigned to that apartment so I know who lives there.

## UC2: Add resident from apartment edit modal

As a condominium attendant, when editing an apartment, I need to add a new resident directly to that apartment so I don't have to navigate to the Residents page.

## UC3: Deactivate resident from apartment edit modal

As a condominium attendant, when editing an apartment, I need to deactivate a resident who no longer belongs there so the resident list stays current.

## UC4: List residents filtered by apartment (backend)

As the system, the residents list endpoint must support an `apartmentId` query parameter so the apartment edit modal can fetch only residents for the selected unit.

## UC5: Fix resident deactivate endpoint (backend)

As the system, `PUT /api/v1/residents/{id}` must accept `active` in the request body so deactivate works correctly (currently sending only `{ active: false }` fails validation because `Name` and `Cpf` are required).