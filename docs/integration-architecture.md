<!--
DEPRECATED — redirect to canonical home.
This file is a `bmad-document-project --mode deep` output from
2026-07-12. It is on a milestone-boundary retirement schedule
(see `.specify/memory/constitution.md` v1.1.0 § "Documentation
Systems and Source of Truth — Retirement schedule").

Canonical home: `AGENTS.md` § 3.3 "Database / DBTools integration"
(multi-tenancy) + `.planning/codebase/INTEGRATIONS.md`.
-->

# Integration Architecture — ControlEasy Reborn

**Project type:** multi-part (`api` + `web` + `db` + reverse proxy + observability)
**Analysis date:** 2026-07-12

## Parts and Communication

| From | To | Type | Protocol | Auth | Where configured |
|------|----|------|----------|------|-----------------|
| Browser | web (nginx) | HTTP | HTTPS via Traefik | none (static SPA) | Traefik label `web.rule=Host(\`localhost\`)` |
| Browser | api (ASP.NET Core) | REST | HTTPS via Traefik | JWT bearer in `Authorization` header | Traefik label `api.rule=Host(\`localhost\`) && PathPrefix(\`/api\`)` |
| web | api (dev) | REST | HTTP (proxy) | JWT | `src/Web/ControlEasyReborn.Web/proxy.conf.json` → `http://localhost:8080` |
| api | db (MySQL) | TCP | MySQL wire | `Db:Username` / `Db:Password` | `src/Host/.../appsettings.json` + `docker/docker-compose.yml` env |
| api | seq (logs) | HTTP | Serilog sink | none (internal network) | `Serilog__WriteTo__1__Args__serverUrl=http://seq:5341` |
| browser | adminer | HTTP | HTTPS via Traefik | dev-only | Traefik label `adminer.rule=PathPrefix(\`/db\`)` |
| browser | traefik dashboard | HTTP | dev-only | none | `--api.insecure=true` on port `8082` |
| external | api | — | none | — | No webhooks, no public inbound |

## System Diagram (cross-part)

```text
            ┌──────────────────────────────────────────┐
            │   Browser (Angular 18 SPA — Part: web)   │
            │   src/Web/ControlEasyReborn.Web          │
            └────────────┬──────────────┬──────────────┘
                         │ /            │ /api/*
                         ▼              ▼
            ┌──────────────────────────────────────────┐
            │   Traefik v3.1  (docker/reverse-proxy)   │
            │   :8080 web  :8443 secure  :8082 api     │
            └────┬───────────┬─────────────┬───────────┘
                 │           │             │
                 ▼           ▼             ▼
        ┌─────────────┐ ┌──────────┐ ┌──────────────┐
        │ web (nginx) │ │  api     │ │   adminer    │
        │  :8080      │ │  :8080   │ │   :8080      │
        │ Angular SPA │ │ ASP.NET  │ │  DB UI       │
        └─────────────┘ └────┬─────┘ └──────────────┘
                             │
              ┌──────────────┼──────────────┐
              │              │              │
              ▼              ▼              ▼
        ┌──────────┐  ┌──────────┐  ┌──────────────┐
        │ db       │  │ seq      │  │  (Redis TBD) │
        │ MySQL 8  │  │ Datallust│  │              │
        │ :3306    │  │ :5341    │  │              │
        └──────────┘  └──────────┘  └──────────────┘
```

## Service Inventory (Docker Compose)

| Service | Image | Internal port | Public exposure | Health check |
|---------|-------|---------------|-----------------|--------------|
| `reverse-proxy` | `traefik:v3.1` | 80, 443, 8080 | host 8080 (HTTP), 8443 (HTTPS), 8082 (dashboard) | n/a |
| `api` | built (`docker/api.Dockerfile`) | 8080 | `/api` via Traefik | implicit (depends_on db) |
| `web` | built (`docker/web.Dockerfile`) | 8080 | `/` via Traefik | implicit |
| `db` | `mysql:8.0` | 3306 | internal only | `mysqladmin ping` |
| `adminer` | `adminer:4` | 8080 | `/db` via Traefik | implicit |
| `seq` | `datalust/seq:latest` | 5341, 80 | internal only | implicit |

**Demo overlay** (`docker/docker-compose.demo.yml`) adds:
- `Demo:Enabled=true` env on `api`
- `Demo__SeedVersion=2` env on `api`
- Optional fixed-credential admin users via `DemoSeederService`

## Auth Flow

1. **Login:** `POST /api/v1/security/auth/login` → `{ accessToken (15 min), refreshToken (7 days) }`
2. **Claims:** `tenant_id`, `profile_id`, `roles[]`, `permissions[]`
3. **Refresh:** `POST /api/v1/security/auth/refresh` with `refreshToken` → new pair (rotation)
4. **Bootstrap:** On first boot (no admin in DB), `PlatformAdminBootstrapService` seeds `platform-admin@controleasy.local` + matching `AttendantProfiles` row. Credentials exposed via `GET /api/v1/security/bootstrap` (E2E-only).
5. **Token storage:** Angular `AuthService` stores tokens in `localStorage` (XSS surface — flagged in `CONCERNS.md`).
6. **Interceptors:** `authInterceptor` attaches the token; `errorInterceptor` surfaces `ProblemDetails` to UI.

## Multi-Tenancy Cross-Cutting

- `TenantResolutionMiddleware` reads `tenant_id` claim → `ITenantContext` (scoped per request).
- `TenantAwareLinqFactory.Create(_ctx)` yields an `IAsyncSqlClient` wrapped in `TenantFilterInterceptor`.
- All tenant-scoped SQL is rewritten at the string level: `WHERE tenant_id = @ctx_tenant` appended.
- Platform-level repositories (auth, tenants registry) inject raw `IAsyncSqlClient` to bypass the filter.
- **Architecture test gap:** no NetArchTest rule enforces the platform/tenant split.

## Data Path Examples

### Authenticated CRUD (Residents list)
1. Angular `ResidentsPage` calls `ResidentsApiService.list({ skip, take, search })`.
2. `HttpClient` → `/api/v1/residents?skip=0&take=20` with `Authorization: Bearer …`.
3. Traefik → `api:8080`.
4. ASP.NET Core: JWT auth → `TenantResolutionMiddleware` populates `ITenantContext`.
5. `MapResidentEndpoints` → `ListResidentsHandler.HandleAsync`.
6. Handler calls `IResidentRepository.ListAsync(skip, take, search, ct)`.
7. Repo uses `TenantAwareLinqFactory.Create(_ctx)` → LINQ expression.
8. DBTools compiles LINQ → SQL; interceptor injects `WHERE tenant_id = @ctx_tenant`.
9. MySQL returns rows → DBTools materializes → handler maps to `ResidentResponse` DTOs.
10. ASP.NET Core serializes JSON → Traefik → Angular.

### Tenant Backup
1. `POST /api/v1/admin/backups` (PlatformAdmin only).
2. `TenantBackupService.CreateBackupAsync` shells out to `mysqldump` + `gzip`.
3. Output written to `Backup:Path` (default `./backups/tenants/`).
4. **Concerns:** full-DB dump, password on argv, host binaries required (not in `api.Dockerfile`).

### Demo Reset
1. `POST /api/v1/demo/reset` (PlatformAdmin only).
2. `DemoSeederService` wipes demo tables and re-seeds.
3. Frontend demo banner (in `src/Web/.../app/layout/`) reminds operator.

## OpenSpec & Project Knowledge

- `openspec/config.yaml` declares `schema: spec-driven` but contains **no `context` block and no `rules`**. There is no `openspec/changes/` directory.
- The **active spec system is `.specs/`** (20 folders, ~191/315 checkbox tasks complete as of 2026-06-24).
- Treat OpenSpec as a forward-compatible slot for the day the team chooses to migrate; for now, all active work follows the `.specs/` + `AGENTS.md` + GSD `.planning/` workflow.

## External Integrations (none)

- **No third-party SaaS:** no Stripe, SendGrid, Twilio, OAuth providers, or external SDKs.
- **No webhooks** (incoming or outgoing).
- **No email/SMS providers** despite `Demo:DisableOutboundEmail` flag existing.

## CI/CD (gap)

- No `.github/workflows/*.yml` present. `.github/` contains only `copilot-instructions.md`.
- The repo's documented "post-task verification" is **manual**: `docker compose build api web` + `dotnet test` + `npm run e2e` per `AGENTS.md`.
- This is captured as a v1 requirement (`CI-01`–`CI-04` in `REQUIREMENTS.md`, deferred to Phase 13).

---

*Integration architecture: 2026-07-12 · Sources: GSD `INTEGRATIONS.md` + `ARCHITECTURE.md` (2026-06-24) + live `docker/docker-compose.yml` cross-check.*
