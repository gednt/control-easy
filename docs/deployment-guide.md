# Deployment Guide — ControlEasy Reborn

**Analysis date:** 2026-07-12

## Deployment Topology

The repository targets **Docker Compose** as the deployment unit (local, dev, demo, and any environment that runs Compose). There is no cloud-specific IaC (Terraform, Helm, k8s manifests) in the repo.

```text
                  Public :8080 / :8443
                          │
                          ▼
                ┌────────────────────┐
                │  Traefik v3.1      │
                │  reverse proxy     │
                └─┬──────┬──────┬────┘
                  │      │      │
            /api/v1  /   │    /db
                  │      │      │
                  ▼      ▼      ▼
              ┌─────┐ ┌─────┐ ┌────────┐
              │ api │ │ web │ │adminer │
              └──┬──┘ └─────┘ └────────┘
                 │
                 ▼
              ┌─────┐
              │ db  │ (MySQL 8.0)
              └─────┘
              ┌─────┐
              │ seq │ (Serilog sink)
              └─────┘
```

## Service Inventory

| Service | Image source | Internal port | Public | Volume |
|---------|--------------|---------------|--------|--------|
| `reverse-proxy` | `traefik:v3.1` | 80, 443, 8080 | host 8080/8443/8082 | `traefik-data` (TLS) |
| `api` | built from `docker/api.Dockerfile` | 8080 | `/api` via Traefik | — |
| `web` | built from `docker/web.Dockerfile` | 8080 | `/` via Traefik | — |
| `db` | `mysql:8.0` | 3306 | internal only | `mysql-data` |
| `adminer` | `adminer:4` | 8080 | `/db` via Traefik | — |
| `seq` | `datalust/seq:latest` | 5341, 80 | internal only | `seq-data` |

## Required Environment Variables (production)

### API container
- `ASPNETCORE_ENVIRONMENT=Production`
- `Db__Provider=MySQL`
- `Db__Host`, `Db__Port`, `Db__Database`, `Db__Username`, `Db__Password`
- `Jwt__SigningKey` (**must override** the default `CHANGE-ME-…` placeholder; min 32 chars)
- `Jwt__Issuer`, `Jwt__Audience`
- Optional: `Serilog__WriteTo__1__Args__serverUrl` (e.g. `http://seq:5341`)

### Compose-level
- `MYSQL_ROOT_PASSWORD`, `MYSQL_USER`, `MYSQL_PASSWORD`
- `JWT_SIGNING_KEY`

### Bootstrap (first boot only)
- `Bootstrap__PlatformAdminEmail`
- `Bootstrap__PlatformAdminPassword` (must be changed on first login)

### Optional
- `Demo__Enabled` (default `false`; **must stay `false` in production**)
- `FeatureManagement__*` (all default `false`; legacy is gone, so flags are dead config)
- `Backup__Path`

## Build & Deploy

```bash
# Pre-warm base images (skips when cached; avoid MCR rate-limits)
for img in \
  mcr.microsoft.com/dotnet/sdk:8.0 \
  mcr.microsoft.com/dotnet/aspnet:8.0 \
  node:20-alpine \
  nginx:alpine \
  mysql:8.0 \
  adminer:4 \
  traefik:v3.1; do
  docker image inspect "$img" >/dev/null 2>&1 || docker pull "$img"
done

# Build only what changed
COMPOSE_DOCKER_CLI_BUILD=1 DOCKER_BUILDKIT=1 \
  docker compose -f docker/docker-compose.yml build api web

# Start / restart
docker compose -f docker/docker-compose.yml up -d --force-recreate api web

# Full reset (re-runs SQL init scripts)
docker compose -f docker/docker-compose.yml down -v
docker compose -f docker/docker-compose.yml up -d --build
```

### Demo overlay
```bash
docker compose -f docker/docker-compose.yml -f docker/docker-compose.demo.yml up -d --build
# Includes seeded tenants + fixed-credential personas. Dev only.
```

## Dockerfiles

### `docker/api.Dockerfile`
- **Stage 1:** `mcr.microsoft.com/dotnet/sdk:8.0` — restore + publish
- **Stage 2:** `mcr.microsoft.com/dotnet/aspnet:8.0` — runtime
- Listens on port `8080` (configurable via `ASPNETCORE_URLS`)

### `docker/web.Dockerfile`
- **Stage 1:** `node:20-alpine` — `npm ci && npm run build`
- **Stage 2:** `nginx:alpine` — serves `dist/controleasy-reborn-web/`
- `docker/nginx.conf` provides SPA fallback (all unknown paths → `index.html`)
- Listens on port `8080` (Traefik routes `/` here)

### Traefik config
- `docker/reverse-proxy/traefik.yml` — static (entrypoints, file provider)
- `docker/reverse-proxy/dynamic.yml` — dynamic routing (`api`, `web`, `adminer`)
- Dashboard: `--api.insecure=true` on port `8082` (**dev only — disable in prod**)

## Health Checks

- `db` service: `mysqladmin ping -h localhost` every 5s, 20 retries
- `api`: no docker healthcheck; rely on `depends_on: condition: service_healthy` for `db`
- Application health: `GET /health` (via `AspNetCore.HealthChecks.MySql`)

## Database Initialization

- `docker/mysql/init/*.sql` runs on first MySQL start (empty volume).
- Ordered: `00-schema.sql` through `11-demo-seed.sql`.
- **No migration framework.** To change schema, add a new `*.sql` with the next sequence number and update `03-tenant-backfill.sql` markers (`SchemaBackfillSyncTests` enforces this).
- **Reset:** `docker compose down -v` clears `mysql-data`; next start re-runs all scripts.

## Operational Concerns (from `CONCERNS.md`)

1. **Default JWT signing key in `appsettings.json`** — must be overridden in production. Consider fail-fast in `Program.cs` when `ASPNETCORE_ENVIRONMENT=Production` and key still matches placeholder.
2. **PlatformAdmin bootstrap logs credentials in plaintext** — review `PlatformAdminBootstrapService.cs` line 86; ideally use the `GET /api/v1/security/bootstrap` endpoint (E2E) and avoid logging the password.
3. **Demo mode fixed password** (`demo123`) — block demo overlay in any production Compose profile.
4. **Adminer and Traefik dashboard exposed in dev** — `/db` and `:8082` are open; restrict or remove for production.
5. **No rate limiting on auth endpoints** — add ASP.NET Core rate limiting middleware or reverse-proxy throttling.
6. **`TenantBackupService` dumps full DB, not tenant-scoped data** — and passes password on argv. Switch to MySQL config file for credentials; per-tenant export recommended.
7. **In-memory dashboard aggregation** — `ReportReadRepository` loads full tables per request; cache or push aggregation to SQL.
8. **`datalust/seq:latest` is unpinned** — pin to a specific version to avoid breaking image updates.
9. **mysqldump/gzip not in `api.Dockerfile`** — `TenantBackupService` will fail at runtime if backups are used.
10. **No CI/CD pipeline** — `.github/workflows/` is empty; all rebuilds are manual (Phase 13 work).

## Post-Deployment Verification (per `AGENTS.md`)

```bash
# After any change, rebuild and verify
docker compose -f docker/docker-compose.yml build api web
docker compose -f docker/docker-compose.yml up -d --force-recreate api web

# Wait for /health to return 200
curl -fsS http://localhost:8080/health

# Smoke-test the API
curl -fsS http://localhost:8080/api/v1/security/bootstrap | jq .

# Run E2E
cd src/Web/ControlEasyReborn.Web && E2E_BASE_URL=http://localhost:8080 npm run e2e
```

## Production Hardening Checklist

- [ ] Override `JWT_SIGNING_KEY` with a real 32+ char secret
- [ ] Set `ASPNETCORE_ENVIRONMENT=Production`
- [ ] Ensure `Demo__Enabled=false`
- [ ] Set strong `MYSQL_ROOT_PASSWORD`, `MYSQL_PASSWORD`
- [ ] Remove or protect Adminer (port 8080 path `/db`)
- [ ] Disable Traefik dashboard (`--api.insecure=false`) or restrict by IP
- [ ] Add rate limiting on `/api/v1/security/*`
- [ ] Pin `datalust/seq` to a versioned tag
- [ ] Configure external log aggregation if Seq is not enough
- [ ] Set up MySQL backups (and per-tenant export if multi-tenant data is sensitive)
- [ ] Pin `datalust/seq`, `traefik`, `mysql`, `adminer`, `nginx` to versioned tags
- [ ] Add HTTPS/TLS (Traefik already supports it on port 8443; provide certs)

## Cloud / Kubernetes

- **Not configured in repo.** To deploy on AWS/Azure/GCP, build the images from `docker/api.Dockerfile` and `docker/web.Dockerfile`, push to a registry, and orchestrate externally.
- The `docker/reverse-proxy/traefik.yml` can be adapted for any container platform that supports Traefik ingress.
- Multi-arch (`linux/arm64`, `windows/arm64`) is **deferred to v2** (Phase 14) per `REQUIREMENTS.md`.

---

*Deployment guide: 2026-07-12 · Sources: GSD `INTEGRATIONS.md` + `CONCERNS.md` (2026-06-24) + live compose files + `AGENTS.md` post-task verification section.*
