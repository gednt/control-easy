# PlatformAdmin First-Boot Login — Bug Analysis

## Overview

First boot of the normal (non-demo) stack runs `PlatformAdminBootstrapService`, which logs random PlatformAdmin credentials. Operators following `docs/demo-mode.md` expect those credentials to sign in at `http://localhost:8080`. Login fails because no `AttendantProfiles` row exists for the seeded user.

## Condition

1. `Demo__Enabled` is false (default compose).
2. Fresh MySQL volume; bootstrap runs once.
3. Operator uses logged email/password on `POST /api/v1/security/auth/login`.
4. `LoginHandler` finds user, loads profiles, finds none active → `401` with message `"No active attendant profile found for user."`

## Example

```text
# API log
// CHANGE IMMEDIATELY — PlatformAdmin seeded. Email: platform-admin-abc12345@controleasy.local, Password: ...

# Login attempt
POST /api/v1/security/auth/login
→ 401 Unauthorized — No active attendant profile found for user.
```

## Fix plan

Extend `PlatformAdminBootstrapService.StartAsync` to insert one `AttendantProfiles` row after the user insert:

| Column | Value |
|---|---|
| `Id` | New GUID |
| `TenantId` / `tenant_id` | `00000000-0000-0000-0000-000000000001` |
| `UserId` | Bootstrapped user id |
| `DisplayName` | `"Platform Admin"` |
| `ShiftId` | `NULL` |
| `GatehouseId` | `NULL` |
| `Permissions` | `platform:*` |
| `Active` | `1` |
| `CreatedAtUtc` | `DateTime.UtcNow` |

Idempotency: when skipping bootstrap because PlatformAdmin user already exists, also skip (no partial state).

## Verification

- Unit or integration test: bootstrap + login returns `200` with token.
- `dotnet test` green.
- Docker rebuild: `docker compose -f docker/docker-compose.yml build api web && docker compose -f docker/docker-compose.yml up -d --force-recreate api web`
