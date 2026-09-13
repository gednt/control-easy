## Context

The `Administration` module was initially scaffolded with minimal generic entities:
1. `ConfigurationEntry`: generic key-value string pairs with no schema enforcement or domain meaning.
2. `AuditLogEntry`: a basic audit table with action/entity columns, but with no cross-module producers writing to it.

Consequently, condominium administrators cannot manage actual operational parameters (e.g. gatehouse rules, visitor entry windows, attendant shift rules, photo consent policies), and the audit trail remains permanently blank.

This design establishes a domain-specific settings model (`CondominiumSettings`), introduces a decoupled cross-module audit logging writer (`IAuditLogWriter`), enhances the audit log query model with rich filtering, and redesigns the frontend Administration page into a cohesive operational hub.

---

## Goals / Non-Goals

### Goals
- Introduce a strongly-typed `CondominiumSettings` domain aggregate and persistence mapping.
- Provide sensible defaults for every tenant so the platform functions out of the box.
- Implement `GET /api/v1/administration/settings` and `PUT /api/v1/administration/settings` with FluentValidation.
- Provide `IAuditLogWriter` in `ControlEasyReborn.SharedKernel` (or BuildingBlocks) to decouple audit writers from the Administration module.
- Wire audit event producers for gatehouse access events, consent refusals, gatehouse overrides, and settings updates.
- Expand `AuditLogEntry` with `Category`, `Severity`, and `MetadataJson`.
- Redesign `AdministrationPage` in Angular with two main tabs:
  - **Condominium & Gatehouse Settings**: organized in operational cards with intuitive form controls (toggles, time pickers, numeric steppers).
  - **System-Wide Audit Trail**: interactive data table with severity badges, category filter, date range filter, and an expandable detail view.

### Non-Goals
- Real-time WebSocket streaming of audit logs (polling or manual refresh on filter change is sufficient).
- External syslog / SIEM exporter integration (kept within MySQL audit store).
- Removing tenant administration (`/tenants`, which is PlatformAdmin-only for multi-condominium provisioning).

---

## Decisions

### Decision 1: Structured `CondominiumSettings` entity mapped via DBTools
**Choice**: Create a dedicated `CondominiumSettings` entity in `Administration.Domain` with explicit properties:
- `VisitDurationMinutes` (int, default 120)
- `RequireShiftHandoverNotes` (bool, default true)
- `DefaultShiftLengthHours` (int, default 8)
- `EmergencyContactPhone` (string?, default null)
- `AllowedVisitorStartHour` (TimeOnly, default 06:00)
- `AllowedVisitorEndHour` (TimeOnly, default 22:00)
- `AutoCheckoutAtMidnight` (bool, default true)
- `MaxActiveVisitorsPerUnit` (int, default 5)
- `PhotoRequiredVisitors` (bool, default true)
- `PhotoRequiredProviders` (bool, default true)
- `PhotoRequiredResidents` (bool, default false)
- `AllowOverrideOnRefusal` (bool, default true)
- `OverdueVisitAlertMinutes` (int, default 15)

**Rationale**: Strongly-typed fields prevent typo bugs, enforce clear business constraints via FluentValidation, and eliminate arbitrary string keys. When a tenant first accesses settings, if no row exists, default settings are seeded transparently.

### Decision 2: Decoupled `IAuditLogWriter` in SharedKernel / BuildingBlocks
**Choice**: Define `IAuditLogWriter` in `BuildingBlocks` with method:
```csharp
Task WriteAsync(
    Guid tenantId,
    string category,
    string action,
    string entityType,
    Guid? entityId,
    AuditSeverity severity,
    string? details,
    object? metadata = null,
    CancellationToken ct = default);
```
Implemented in `ControlEasyReborn.Modules.Administration.Infrastructure` or `BuildingBlocks`.

**Rationale**: Modules (`Visits`, `Photos`, `Security`, `Residents`) can record security and operational events without taking a project reference on `Administration`. This satisfies Clean Architecture and prevents circular dependencies.

### Decision 3: Audit Severity and Category Enumerations
**Choice**: Define standardized enums:
- `AuditCategory`: `Gatehouse`, `Visits`, `Residents`, `Apartments`, `Security`, `Settings`, `System`
- `AuditSeverity`: `Info`, `Warning`, `SecurityAlert`

**Rationale**: Categorizing events allows the administrator to filter out high-volume routine operations (e.g. routine visitor checkout) and focus on security-relevant occurrences (e.g. consent refusal, gatehouse override, password reset, settings modification).

### Decision 4: Redesigned Angular UI with CeTabs and Design System Tokens
**Choice**: The redesigned `CeAdministrationPageComponent` utilizes the established design system tokens (`--color-primary`, `--color-surface`, `--shadow-card`, etc.) and presents:
- A clean tab switch between "Condominium & Gatehouse Settings" and "System-Wide Audit Trail".
- Settings form powered by Angular Reactive Forms and signals, featuring visual groupings, switch toggles, duration inputs, and a sticky "Save Changes" action bar.
- Audit table featuring a filter toolbar (Category dropdown, Severity filter pills, date range selector, text search), clear severity badges, and a side-sheet or modal for event payload inspection.

---

## Risks / Trade-offs

| Risk | Mitigation |
|---|---|
| First-time tenant access to settings has no row | Handler returns default instance and seeds the row if missing |
| High volume of audit logs slowing queries | Index `(tenant_id, created_at_utc)` and `(tenant_id, category, severity)` in MySQL; enforce default pagination (max 50 per page) |
| Module decoupling violation | Use `IAuditLogWriter` interface in shared kernel; NetArchTest validates no circular dependencies |
