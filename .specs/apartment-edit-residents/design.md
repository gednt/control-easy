# Design -- Apartment edit: manage residents

## Backend changes

### Apartment filter on residents list

`GET /api/v1/residents?apartmentId={guid}` adds an optional filter. The `ListAsync` method in `IResidentRepository` and `ResidentRepository` gains a `Guid? apartmentId` parameter. When provided, `AND ApartmentId = @paramN` is appended to the WHERE clause. The endpoint and handler pass it through.

### Fix resident deactivate

`UpdateResidentRequest` gains `bool Active`. `UpdateResidentHandler` checks `!request.Active` and calls `resident.Deactivate()`. The Angular service sends the full resident payload with `active: false`.

## Frontend changes

### Residents API service

- `list()` gains optional `apartmentId` parameter
- `deactivate()` sends full `UpdateResidentRequest` payload with `active: false`

### Apartments edit modal

The edit modal expands to include:

1. **Residents section** below the block/unit form showing active residents for the apartment
2. **Inline add form** with Name, CPF, Phone fields (apartment is implicit)
3. **Deactivate button** per resident row with confirmation dialog

Permissions: list visible with `Residents.Read`, add/deactivate visible with `Residents.Write`.