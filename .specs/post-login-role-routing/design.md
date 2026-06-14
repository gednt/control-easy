# Design — Tenant-Scoped Access

## Overview

Porteiros and tenant admins must operate strictly within their assigned condominium(s). Platform admins manage all condominiums from `/platform/condominiums`. Backend JWT `tenant_id` already scopes API reads/writes; this feature tightens login/profile selection, tenant switch authorization, and the Angular shell.

## Backend

- `GET /api/v1/security/session` — returns current tenant metadata and `switchableTenants` (only condominiums where the user has an active profile; excludes platform tenant for non-platform users).
- Login / refresh pick the first active **condominium** profile for non-platform users (skip platform tenant `000…001`).
- Tenant lookup and tenant-switch reject platform tenant for non-platform users.

## Frontend

- `TenantSessionService` loads session after authentication.
- Topbar shows condominium name; multi-tenant users get a switcher limited to their assigned condos.
- Sidebar footer shows user + condominium; tenant users see portaria modules only; platform admin sees Platform section only.
- Post-login routing: Platform Admin → `/platform/condominiums`; others → `/`.

## Verification

- `porteiro@controleasy.app` → dashboard, topbar shows `[Demo] Residencial Aurora`, 61 residents.
- `admin@controleasy.app` → same tenant scope.
- `multi@controleasy.app` → switcher with exactly 2 demo condos.
- `platform@controleasy.app` → `/platform/condominiums`, no tenant switcher.
