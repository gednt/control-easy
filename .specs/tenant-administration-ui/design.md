# Design — Tenant Administration UI

## Overview

Delivers the Angular UI for modernization-roadmap task **3.9**, consuming the existing Tenants module API (task **3.9a**) plus one new list endpoint.

## API surface

| Method | Route | Auth | Purpose |
|---|---|---|---|
| `GET` | `/api/v1/tenants` | PlatformAdmin | List all condominiums |
| `POST` | `/api/v1/tenants` | PlatformAdmin | Register condominium |
| `POST` | `/api/v1/tenants/{id}/suspend` | PlatformAdmin | Suspend |
| `POST` | `/api/v1/tenants/{id}/resume` | PlatformAdmin | Resume |
| `POST` | `/api/v1/tenants/{id}/admins` | PlatformAdmin | Create tenant admin |

## Frontend

- Route: `/platform/condominiums`
- Visible in sidebar only when JWT roles include `PlatformAdmin`
- Page sections:
  - Header + "Register condominium" action
  - Table: display name, slug, status, created date, actions (suspend/resume)
  - Modal form: slug, display name, optional first admin (email, display name, password)
- Reuse existing design tokens (`ce-button`, `ce-input`, page layout patterns from apartments/residents pages)
- API client: `tenants-api.service.ts` under `features/platform/`

## Backend addition

- `ListTenantsHandler` + `ITenantRepository.ListAsync` ordered by `CreatedAtUtc` descending
- `GET /` on existing `/api/v1/tenants` group (PlatformAdmin policy already applied to group)

## Out of scope (v1)

- Per-tenant backup trigger from UI (`POST /api/v1/admin/backups/{tenantId}` remains API-only)
- Tenant admin revoke UI (API exists; can be a follow-up)
- Pagination (simple full list is sufficient for tens of condominiums)
