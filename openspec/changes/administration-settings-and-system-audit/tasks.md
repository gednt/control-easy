# Tasks — Administration Settings and System Audit

> **Scope.** Replace generic configuration and dead audit ledger with structured Condominium & Gatehouse Settings and an interactive System-Wide Security Audit Trail across backend and Angular SPA.

## 1. Backend Domain & Infrastructure

- [x] 1.1 Create `CondominiumSettings` domain aggregate in `ControlEasyReborn.Modules.Administration.Domain.Entities` with gatehouse operations, visitor access, photo consent policies, and alert threshold properties.
- [x] 1.2 Enhance `AuditLogEntry` in `ControlEasyReborn.Modules.Administration.Domain.Entities` to include `Category`, `Severity`, and `MetadataJson`. Define `AuditCategory` and `AuditSeverity` enums.
- [x] 1.3 Define `IAuditLogWriter` in `ControlEasyReborn.SharedKernel` (or `BuildingBlocks`) to decouple cross-module audit logging. Implement the writer in `Administration.Infrastructure`.
- [x] 1.4 Add database table definitions or migrations for `condominium_settings` and update `audit_log_entries` in MySQL init scripts and repositories.

## 2. Backend Application Handlers & Minimal API

- [x] 2.1 Implement `GetCondominiumSettingsHandler` and `UpdateCondominiumSettingsHandler` with FluentValidation constraints (`CondominiumSettingsValidator`).
- [x] 2.2 Enhance `ListAuditLogHandler` in `Administration.Application` to support filtering by `category`, `severity`, `fromUtc`, `toUtc`, `searchTerm`, and pagination.
- [x] 2.3 Map API routes in `AdministrationEndpoints.cs`:
  - `GET /api/v1/administration/settings`
  - `PUT /api/v1/administration/settings`
  - Enhanced `GET /api/v1/administration/audit-logs`
  - `GET /api/v1/administration/audit-logs/{id}`
- [x] 2.4 Wire `IAuditLogWriter` into key platform events (e.g. `EntryRefused`, `OverrideAuthorized` in gatehouse workflow, `SettingsUpdated` in administration).

## 3. Frontend Service & UI Redesign

- [x] 3.1 Update `administration-api.service.ts` in Angular SPA with DTOs and methods for `getSettings()`, `updateSettings()`, and `listAuditLogs(query)`.
- [x] 3.2 Redesign `AdministrationPage` (`administration.page.ts`) to provide two dedicated tabs:
  - **Condominium & Gatehouse Settings** tab: card-based layout for Gatehouse Operations, Visitor Access, Photo Policies, and Alerts with reactive form controls, dirty state tracking, and save feedback.
  - **System-Wide Audit Trail** tab: interactive filter toolbar (date range, category dropdown, severity toggles, search text), data table with severity badges, and event detail modal.
- [x] 3.3 Apply design system tokens and responsive layouts so both tabs render seamlessly on desktop and tablet viewports.

## 4. Testing & Verification

- [x] 4.1 Write unit tests for `CondominiumSettingsValidator` and settings handlers in `ControlEasyReborn.UnitTests`.
- [x] 4.2 Write unit tests for enhanced audit log filtering in `ControlEasyReborn.UnitTests`.
- [x] 4.3 Write Angular unit tests for `AdministrationPage` verifying tab switching, settings form validation, and audit filter reactivity.
- [x] 4.4 Run architectural tests (`ControlEasyReborn.ArchitectureTests`) to confirm no forbidden dependencies or layer leaks.
- [x] 4.5 Build full solution (`docker compose build api web`) and verify running services.

## Task Dependency Graph

```json
{
  "waves": [
    {
      "wave": 1,
      "tasks": ["1.1", "1.2", "1.3", "1.4"]
    },
    {
      "wave": 2,
      "tasks": ["2.1", "2.2", "2.3", "2.4"]
    },
    {
      "wave": 3,
      "tasks": ["3.1", "3.2", "3.3"]
    },
    {
      "wave": 4,
      "tasks": ["4.1", "4.2", "4.3", "4.4", "4.5"]
    }
  ]
}
```
