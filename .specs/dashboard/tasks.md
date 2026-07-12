# Dashboard Implementation Tasks

## Overview
Implement a real dashboard page that displays live statistics for the condominium. Currently the dashboard page is a static placeholder showing only "Getting started" text.

## Current State
- Frontend: `src/Web/ControlEasyReborn.Web/src/app/features/dashboard/dashboard.page.ts` - static placeholder
- Backend: No stats/dashboard endpoint exists
- Other modules have CRUD endpoints but no aggregate stats endpoint

## Task Dependency Graph
```json
{
  "waves": [
    { "wave": 1, "tasks": ["T1"] },
    { "wave": 2, "tasks": ["T2"] },
    { "wave": 3, "tasks": ["T3"] }
  ]
}
```

## Tasks

### T1 - Create Dashboard Stats Backend Endpoint
Create a `GET /api/v1/dashboard/stats` endpoint that returns aggregate counts and recent activity.

**Response DTO:**
```csharp
public sealed record DashboardStatsResponse(
    int TotalResidents,
    int ActiveResidents,
    int TotalVehicles,
    int ActiveVehicles,
    int TotalApartments,
    int OccupiedApartments,
    int OpenVisits,
    int TodayVisits,
    List<RecentVisitDto> RecentVisits
);

public sealed record RecentVisitDto(
    Guid Id,
    string VisitorName,
    string? Purpose,
    string Status,
    string? ApartmentLabel,
    DateTime CreatedAtUtc
);
```

**Implementation notes:**
- Add a `Dashboard` module or add the endpoint to an existing module (e.g., Reports)
- Use existing repositories from Residents, Vehicles, Visits, Apartments modules to query counts
- The endpoint should require authorization
- Follow the existing pattern: `DashboardEndpoints.cs` + `DashboardStatsHandler.cs` + DI extension
- Register in `Program.cs`

### T2 - Create Frontend Dashboard API Service
Create `dashboard-api.service.ts` with a `getStats()` method that calls `GET /api/v1/dashboard/stats`.

**TypeScript interface:**
```typescript
export interface DashboardStatsResponse {
  totalResidents: number;
  activeResidents: number;
  totalVehicles: number;
  activeVehicles: number;
  totalApartments: number;
  occupiedApartments: number;
  openVisits: number;
  todayVisits: number;
  recentVisits: RecentVisit[];
}

export interface RecentVisit {
  id: string;
  visitorName: string;
  purpose: string | null;
  status: string;
  apartmentLabel: string | null;
  createdAtUtc: string;
}
```

### T3 - Implement Dashboard Page UI
Replace the placeholder dashboard with a real dashboard showing:
- Stat tiles: Active Residents, Active Vehicles, Occupied Apartments, Open Visits
- Recent visits table (last 5-10 visits)
- Each stat tile links to the corresponding feature page
- Loading states, error states
- Follow the design system used in residents/visits pages
- Use `CeCardComponent` and `stat-tile` design system component if available