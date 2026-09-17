# Data Model: Dashboard Visits and Access Registration

## Visit

- Existing visit records retain visitor identification, receiving apartment, purpose, and lifecycle timestamps.
- New records created by an operator through the shared form require a receiving apartment.
- Access audit records are not visit records and are not copied into this entity.

## Access audit event

- Existing append-only, tenant-scoped event record.
- Fields used by this change: event state, subject category, optional identifying name/document, recording profile, and timestamp.
- New state: `exited`.
- Exit is valid only for the resident and vehicle subject categories.

## Relationships and validation

- A Visit refers to one existing apartment.
- An access audit event belongs to one tenant and optionally identifies the gatehouse profile that recorded it.
- A successful exit creates a new event; it never modifies a prior event.
