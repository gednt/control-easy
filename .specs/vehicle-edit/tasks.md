# Vehicle Edit Implementation Tasks

## Overview
Add vehicle editing functionality. The backend `PUT /api/v1/vehicles/{id}` endpoint and `UpdateVehicleHandler` already exist. The frontend needs: an `update()` method in the API service, an edit modal in the vehicles page, and edit action buttons in the table rows.

## Current State
- Backend: `PUT /api/v1/vehicles/{id}` with `UpdateVehicleRequest` exists (Plate, Brand, Model, Color, ApartmentId, OwnerName, VehicleType)
- Backend: `UpdateVehicleHandler` validates and updates vehicles
- Frontend API service: Only has `list()` and `create()` - missing `update()` and `get()`
- Frontend page: Only has create modal - missing edit modal and edit buttons
- Reference: The residents page (`residents.page.ts`) has the full pattern with edit modal, deactivate confirm, etc.

## Task Dependency Graph
```json
{
  "waves": [
    { "wave": 1, "tasks": ["T1"] },
    { "wave": 2, "tasks": ["T2"] }
  ]
}
```

## Tasks

### T1 - Add update() and get() to VehiclesApiService
Add to `vehicles-api.service.ts`:

```typescript
get(id: string): Observable<VehicleResponse> {
  return this.http.get<VehicleResponse>(`${this.baseUrl}/${id}`);
}

update(id: string, request: UpdateVehicleRequest): Observable<VehicleResponse> {
  return this.http.put<VehicleResponse>(`${this.baseUrl}/${id}`, request);
}
```

Also add the `UpdateVehicleRequest` interface:
```typescript
export interface UpdateVehicleRequest {
  plate: string;
  brand?: string | null;
  model?: string | null;
  color?: string | null;
  apartmentId?: string | null;
  ownerName?: string | null;
  vehicleType?: string | null;
}
```

### T2 - Add Edit Modal and Edit Button to Vehicles Page
Mirror the pattern from `residents.page.ts`:
- Add an edit button (pencil icon) in each table row's action column
- Add an edit modal with form fields: Plate (required), Owner, Apartment (picker), Brand, Model, Color, VehicleType (dropdown)
- Add `editForm` FormGroup, `editModalOpen` signal, `editingVehicle` signal, `editError` signal, `saving` signal
- Add `openEditModal(vehicle)`, `closeEditModal()`, `onEditVehicle()` methods
- Populate form with existing vehicle data when opening edit
- Add vehicle type dropdown (Car, Motorcycle, Truck, Other) to both create and edit forms
- Also add a deactivate button (with confirmation) for active vehicles, mirroring the residents pattern
- Add permission checks using `AuthService.hasPermission('Vehicles.Write')`
- Follow the same design system styles (ce-button, ce-modal, ce-form, etc.)