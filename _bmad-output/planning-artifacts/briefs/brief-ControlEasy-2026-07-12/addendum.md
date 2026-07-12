# Addendum — ControlEasy Reborn Demo Mode

> Material that informed the brief but belongs in downstream artifacts (PRD, architecture, solution design) or that didn't fit the brief. Captured during the run on 2026-07-12.

## A. Curated demo resident roster (canonical contract)

This roster is the **spec-level contract** for the demo. Implementers must not substitute names, split canonical Chaves households across apartments, or swap in Norse-era God of War characters (Atreus is the documented exception).

### `[Demo] Residencial Aurora` — 61 residents, tenant `demo-aurora`

**Distribution:** 20 Chaves + 20 GTA + 21 God of War (Greek era + Atreus) = **61**.

**Cohabitation rules (must be enforced in `DemoFixtures.cs`):**

- **apt 8** — Dona Florinda, Quico, Prof. Girafales (Chaves; Florinda's apartment)
- **apt 14** — Seu Madruga, Chilindrina, Chiquinha (Chaves; Madruga household)
- **apt 7** — Jaça, Pingüinos (Chaves; neighbourhood kids' hangout)
- **apt 18** — Maruxa, Serafim (Chaves; elderly couple)
- **apt 1** — Sr. Barriga (Chaves; landlord, solo)
- **Barril** — Chaves (solo, in the courtyard barrel)
- **Sparta-1** — Kratos, Atreus, Deimos, Callisto (God of War; Spartan family home)
- **Underworld-1** — Hades, Persephone (God of War; mythic household)
- **Olympus-3** — Ares, Aphrodite (God of War; Olympian wing)
- All other listed characters have their own apartment.

| # | Name | Franchise | Block | Apt | Status |
|---|------|-----------|-------|-----|--------|
| 1 | Chaves | Chaves | A | Barril | active |
| 2 | Dona Florinda | Chaves | A | 8 | active |
| 3 | Quico | Chaves | A | 8 | active |
| 4 | Prof. Girafales | Chaves | A | 8 | active |
| 5 | Seu Madruga | Chaves | A | 14 | overdue |
| 6 | Chilindrina | Chaves | A | 14 | active |
| 7 | Chiquinha | Chaves | A | 14 | active |
| 8 | Sr. Barriga | Chaves | A | 1 | pending |
| 9 | Popis | Chaves | A | 11 | active |
| 10 | Paty | Chaves | A | 10 | inactive |
| 11 | Godinez | Chaves | A | 12 | pending |
| 12 | Jaça | Chaves | A | 7 | active |
| 13 | Pingüinos | Chaves | A | 7 | active |
| 14 | Jaiminho | Chaves | A | Correios | active |
| 15 | Dona Neves | Chaves | A | 15 | pending |
| 16 | Gloria | Chaves | A | 16 | active |
| 17 | Botija | Chaves | A | 17 | inactive |
| 18 | Maruxa | Chaves | A | 18 | active |
| 19 | Serafim | Chaves | A | 18 | overdue |
| 20 | Pelón | Chaves | A | 13 | active |
| 21 | Michael De Santa | GTA | B | 101 | active |
| 22 | Franklin Clinton | GTA | B | 102 | active |
| 23 | Trevor Philips | GTA | B | 103 | overdue |
| 24 | CJ | GTA | B | 201 | active |
| 25 | Tommy Vercetti | GTA | B | 202 | active |
| 26 | Niko Bellic | GTA | B | 203 | pending |
| 27 | Lamar Davis | GTA | B | 301 | active |
| 28 | Lester Crest | GTA | B | 302 | inactive |
| 29 | Roman Bellic | GTA | B | 303 | active |
| 30 | Sweet Johnson | GTA | B | 401 | active |
| 31 | Amanda De Santa | GTA | B | 104 | active |
| 32 | Lucia De Santa | GTA | B | 105 | active |
| 33 | Wade | GTA | B | 204 | active |
| 34 | Stretch | GTA | B | 205 | overdue |
| 35 | Big Smoke | GTA | B | 304 | active |
| 36 | Ryder | GTA | B | 305 | pending |
| 37 | Tenpenny | GTA | B | 402 | inactive |
| 38 | Woozie | GTA | B | 403 | active |
| 39 | Lance Vance | GTA | B | 404 | active |
| 40 | Ken Rosenberg | GTA | B | 405 | pending |
| 41 | Kratos | God of War (Greek) | C | Sparta-1 | active |
| 42 | Atreus | God of War (Greek) | C | Sparta-1 | active |
| 43 | Deimos | God of War (Greek) | C | Sparta-1 | active |
| 44 | Callisto | God of War (Greek) | C | Sparta-1 | inactive |
| 45 | Athena | God of War (Greek) | C | Olympus-2 | active |
| 46 | Ares | God of War (Greek) | C | Olympus-3 | inactive |
| 47 | Aphrodite | God of War (Greek) | C | Olympus-3 | pending |
| 48 | Hephaestus | God of War (Greek) | C | Forge-1 | active |
| 49 | Hermes | God of War (Greek) | C | Olympus-4 | overdue |
| 50 | Hades | God of War (Greek) | C | Underworld-1 | active |
| 51 | Persephone | God of War (Greek) | C | Underworld-1 | active |
| 52 | Poseidon | God of War (Greek) | C | Sea-1 | active |
| 53 | Helios | God of War (Greek) | C | Sun-1 | active |
| 54 | Perseus | God of War (Greek) | C | Hero-1 | inactive |
| 55 | Theseus | God of War (Greek) | C | Hero-2 | active |
| 56 | Hercules | God of War (Greek) | C | Hero-3 | active |
| 57 | Orpheus | God of War (Greek) | C | Hero-4 | active |
| 58 | Pandora | God of War (Greek) | C | Hero-5 | pending |
| 59 | Gaia | God of War (Greek) | C | Titan-1 | overdue |
| 60 | Cronos | God of War (Greek) | C | Titan-2 | pending |
| 61 | Zeus | God of War (Greek) | D | Pantheon-601 | inactive |

**Status → domain mapping:** `active` → `Active=true`; `inactive` → `Active=false`; `pending` / `overdue` → `Active=true` with a `ResidentStatus` tag (used by the UI badge).

### `[Demo] Condomínio Parque Verde` — 6 residents, tenant `demo-parque-verde`

Secondary set, characters **not** duplicated in Aurora, so the multi-tenant picker still shows themed data.

| # | Name | Franchise | Block | Apt | Status |
|---|------|-----------|-------|-----|--------|
| 1 | Rogelio | Chaves | A | 110 | active |
| 2 | Úrsulo | Chaves | A | 111 | active |
| 3 | Denise | GTA | B | 210 | active |
| 4 | Mallorie | GTA | B | 211 | pending |
| 5 | Eurydice | God of War (Greek) | C | 310 | active |
| 6 | Charon | God of War (Greek) | C | 311 | inactive |

## B. Personas & credentials (full table)

| Email | Password | Role | Tenant(s) |
|-------|----------|------|-----------|
| `platform@controleasy.app` | `demo123` | PlatformAdmin | platform |
| `admin@controleasy.app` | `demo123` | TenantAdmin | `[Demo] Residencial Aurora` |
| `porteiro@controleasy.app` | `demo123` | AttendantProfile | `[Demo] Residencial Aurora` |
| `morador@controleasy.app` | `demo123` | Morador | `[Demo] Residencial Aurora` |
| `multi@controleasy.app` | `demo123` | TenantAdmin + Attendant | `[Demo] Residencial Aurora` + `[Demo] Condomínio Parque Verde` |

## C. Fixed tenant GUIDs (stable across re-seeds)

| Tenant | Id | Slug | DisplayName |
|--------|------|------|-------------|
| Platform (system) | `00000000-0000-0000-0000-000000000001` | `platform` | Platform |
| Demo — Aurora | `00000000-0000-0000-0000-000000000010` | `demo-aurora` | `[Demo] Residencial Aurora` |
| Demo — Parque Verde | `00000000-0000-0000-0000-000000000020` | `demo-parque-verde` | `[Demo] Condomínio Parque Verde` |

## D. Configuration & env-var reference

`appsettings.json` defaults (demo **off** by default):

```json
{
  "Demo": {
    "Enabled": false,
    "SeedVersion": 1,
    "DisableOutboundEmail": false
  }
}
```

Demo overlay env vars (`docker-compose.demo.yml`):

| Variable | Demo value | Purpose |
|----------|------------|---------|
| `Demo__Enabled` | `true` | Master switch |
| `Demo__SeedVersion` | `1` | Bump to force re-seed |
| `Demo__DisableOutboundEmail` | `true` | Block SMTP / notifications |
| `FeatureManagement__{Res,Vis,Veh,SP,Admin}__UseWeb` | `true` | All web modules enabled |
| `Jwt__SigningKey` | `demo-signing-key-not-for-production-use!!` | Predictable local demo only |

## E. Database schema (delta)

New platform table (no `tenant_id`) added to `docker/mysql/init/10-demo-metadata.sql`:

```sql
CREATE TABLE IF NOT EXISTS DemoMetadata (
    Id           INT          NOT NULL PRIMARY KEY DEFAULT 1,
    SeedVersion  INT          NOT NULL DEFAULT 0,
    SeededAtUtc  DATETIME(6)  NOT NULL,
    CHECK (Id = 1)
) ENGINE=InnoDB;
```

Must be added to the **backfill exemption list** in architecture tests (or `SchemaBackfillSyncTests` will fail). Optional minimal `docker/mysql/init/11-demo-seed.sql` for Testcontainers that boot without the hosted seeder (tenants + users only — full fixture via `DemoSeederService`).

## F. API endpoints (full reference)

| Method | Route | Auth | Behaviour |
|--------|-------|------|-----------|
| `GET` | `/api/v1/demo/info` | Anonymous | `{ "enabled": bool, "seedVersion": int, "tenants": [{ "slug", "displayName" }] }`. When disabled: `{ "enabled": false }`. |
| `POST` | `/api/v1/demo/reset` | PlatformAdmin | Demo mode only. Truncates demo tenant business data, re-runs seeder, returns 204. Returns 404 when demo disabled. Both return `ProblemDetails` on error. |

## G. Docker Compose overlay (current draft)

```yaml
services:
  api:
    environment:
      Demo__Enabled: "true"
      Demo__SeedVersion: "1"
      Demo__DisableOutboundEmail: "true"
      FeatureManagement__Residents__UseWeb: "true"
      FeatureManagement__Visits__UseWeb: "true"
      FeatureManagement__Vehicles__UseWeb: "true"
      FeatureManagement__ServiceProviders__UseWeb: "true"
      FeatureManagement__Administration__UseWeb: "true"
      Jwt__SigningKey: demo-signing-key-not-for-production-use!!
  web:
    environment:
      NGINX_ENVSUBST_OUTPUT_DIR: /etc/nginx
```

Documented command:

```bash
docker compose -f docker/docker-compose.yml -f docker/docker-compose.demo.yml up -d --build
```

Full reset:

```bash
docker compose -f docker/docker-compose.yml -f docker/docker-compose.demo.yml down -v
docker compose -f docker/docker-compose.yml -f docker/docker-compose.demo.yml up -d --build
```

## H. Frontend components (planned)

- `core/services/demo-info.service.ts` — fetches `/api/v1/demo/info` once at app init; exposes `enabled`, `tenants`, `seedVersion` as Angular signals.
- `layout/demo-banner.component.ts` (selector `ce-demo-banner`) — visible when `demoInfo.enabled()`; dismissible per session via `sessionStorage['ce.demoBanner.dismissed']`; uses `ce-badge tone-warning` styling.
- `features/login.page.ts` — collapsible "Try a demo account" panel below the login form, demo-only; pre-fills email and shows `demo123` as a `readonly` hint, does **not** auto-submit.
- `features/demo-help.page.ts` — lazy route `/help/demo`; redirect to `/` when demo disabled.
- User menu: add "Help → Demo guide" when demo enabled.

## I. Security guardrails (from design.md)

1. Demo endpoints return `404` when `Demo__Enabled=false`.
2. Demo seed runs only when enabled; no demo users in non-demo deployments.
3. `Demo__DisableOutboundEmail` prevents accidental mail in demo environments.
4. Documented JWT key is **demo-only** — production compose must never use the overlay.
5. `POST /api/v1/demo/reset` is audit-logged when the Administration audit module is available.

## J. Implementation status snapshot (as of 2026-07-12)

- **All 10 numbered tasks (8.1–8.10) are complete on disk** (verified 2026-07-12 via live codebase review: `DemoOptions`, `DemoFixtures`, `DemoSeederService`, `DemoEndpoints`, `docker-compose.demo.yml`, `DemoInfoService`, `DemoBannerComponent`, login shortcuts, `/help/demo` route, `DemoModeTests`, architecture-test exemption for `DemoMetadata`, and `docs/demo-mode.md`).
- **Spec checkboxes reconciled:** `.specs/4 - demo-mode/tasks.md` was updated to mark all 10 tasks as `[x]` with inline "Verified 2026-07-12" notes pointing to the concrete files.
- **Planning docs updated:** `ROADMAP.md` now shows "10/10 Complete" for Phase 11 (Demo Mode); `REQUIREMENTS.md` ticks DEMO-01 and DEMO-02 as complete.
- **Recommended next action:** Confirm no remaining work, or close out Phase 11 entirely and move to Phase 8 (Design System).

## K. Risks (from spec design.md)

| Risk | Mitigation |
|------|------------|
| Seed drift when module schemas change | `Demo__SeedVersion` bump + integration test asserting resident count = 61 for `demo-aurora` and cohabitation groups |
| Demo credentials leaked to production | Overlay file clearly named; CI lint checks `Demo__Enabled` not set in prod workflow |
| Seeder slows startup | Idempotent skip when version matches; seed only on first boot or reset |

## L. Out of scope (full list, for downstream consumers)

- Public internet-facing demo SaaS (TLS, rate limiting, WAF) — future DevOps spec
- Anonymized import of real legacy `controlEasyDB.db` data — separate data-migration spec
- Demo mode for the legacy WPF app — web stack only
- Synthetic MQTT / hardware simulation — deferred to `.specs/3 - photo-capture-hardware-integration/`
- Billing, subscription, or trial-expiry flows
- Automatic demo data generation via LLM — static curated fixtures only
- Re-skinning the demo for non-pt-BR locales

---

*Addendum authored 2026-07-12 by `bmad-product-brief` (headless). All content sourced from `.specs/4 - demo-mode/{requirements,design}.md` and `docs/demo-mode.md`. No rejected alternatives or parked-roadmap content was generated during this headless run.*
