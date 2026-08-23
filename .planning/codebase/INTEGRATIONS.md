# External Integrations

**Analysis Date:** 2026-06-24

## APIs & External Services

**Third-party SaaS:**
- Not detected — no Stripe, SendGrid, Twilio, OAuth providers, or other external SaaS SDKs in `src/` or `tests/`
- `Demo:DisableOutboundEmail` flag exists in `src/BuildingBlocks/ControlEasyReborn.SharedKernel/Demo/DemoOptions.cs` and `docker/docker-compose.demo.yml`, but no email/SMS sending implementation is present in the codebase

**Internal REST API (Angular ↔ ASP.NET Core):**
- ControlEasy Reborn API — primary integration surface for the Angular SPA
  - Client: Angular `HttpClient` with interceptors in `src/Web/ControlEasyReborn.Web/src/app/core/interceptors/auth.interceptor.ts` and `error.interceptor.ts`
  - Auth: JWT Bearer token in `Authorization` header (stored in `src/Web/ControlEasyReborn.Web/src/app/core/services/auth.service.ts`)
  - Dev routing: `src/Web/ControlEasyReborn.Web/proxy.conf.json` → `http://localhost:8080`
  - Docker routing: Traefik in `docker/reverse-proxy/dynamic.yml` — `/api` → `api:8080`, `/` → `web:8080`

**OpenAPI contract:**
- Swashbuckle — generates Swagger at `/swagger` in Development (`src/Host/ControlEasyReborn.Api/Program.cs`)
- ng-openapi-gen — generates TypeScript API client from `https://localhost/swagger/v1/swagger.json` into `src/Web/ControlEasyReborn.Web/src/app/api/` (`ng-openapi-gen.json`, `npm run openapi-gen`)

**Legacy systems (documented, not in repo):**
- ControlEasy 5 WPF desktop — legacy `.NET Framework 4.8` app with direct MySQL via Entity Framework 6 and `MySql.Data` (per `AGENTS.md`); **source not present in this repository**
- ControlEasyWeb Blazor Server — half-started `.NET 5` prototype referencing legacy WPF (per `AGENTS.md`); **source not present in this repository**

## Data Storage

**Databases:**
- MySQL 8.0 — primary data store for ControlEasy Reborn
  - Container: `mysql:8.0` service in `docker/docker-compose.yml`
  - Connection: `Db:Host`, `Db:Port`, `Db:Database`, `Db:Username`, `Db:Password`, `Db:Provider` (env vars or `src/Host/ControlEasyReborn.Api/appsettings.json`)
  - Client/ORM: DBTools_SQL `IAsyncSqlClient` / `Linq<TModel>` via `src/BuildingBlocks/ControlEasyReborn.Infrastructure/Data/ServiceCollectionExtensions.cs`
  - MySQL driver: MySqlConnector loaded reflectively by `src/lib/DBTools_SQL/DBTools/Providers/MySqlProvider.cs`
  - Schema/migrations: SQL init scripts in `docker/mysql/init/` (e.g. `00-schema.sql`, `02a-residents-schema.sql`, `05-security-schema.sql`, `11-demo-seed.sql`)
  - Health check: `/health` via `AspNetCore.HealthChecks.MySql` in `Program.cs`
  - Test isolation: Testcontainers MySQL 8.0 in `tests/ControlEasyReborn.IntegrationTests/MySqlContainerFixture.cs`

**File Storage:**
- Local filesystem — tenant SQL backups written to `Backup:Path` (default `./backups/tenants/`) via `mysqldump` + `gzip` in `src/Modules/Tenants/ControlEasyReborn.Modules.Tenants.Infrastructure/Persistence/TenantBackupService.cs`
- No cloud object storage (S3, Azure Blob, etc.) detected

**Caching:**
- None — Redis is mentioned as optional in `AGENTS.md` and `.specs/1 - modernization-roadmap/design.md` but no Redis client, service, or compose service exists in the codebase

## Authentication & Identity

**Auth Provider:**
- Custom JWT (symmetric HMAC-SHA256) — no Auth0, Azure AD, or OAuth2 identity provider
  - Implementation: `Microsoft.AspNetCore.Authentication.JwtBearer` in `src/Host/ControlEasyReborn.Api/Program.cs`
  - Token issuance: `src/Modules/Security/ControlEasyReborn.Modules.Security.Infrastructure/Persistence/JwtTokenService.cs`
  - Login endpoint: `POST /api/v1/security/auth/login` in `src/Modules/Security/ControlEasyReborn.Modules.Security.Api/Endpoints/SecurityEndpoints.cs`
  - Refresh endpoint: `POST /api/v1/security/auth/refresh`
  - Password hashing: BCrypt via `src/Modules/Security/ControlEasyReborn.Modules.Security.Infrastructure/Persistence/PasswordHasher.cs`
  - Refresh tokens: persisted in MySQL via `IRefreshTokenRepository` in Security Infrastructure
  - Multi-tenancy: JWT claims `tenant_id`, `profile_id`, `roles`, `permissions`; resolved by `TenantResolutionMiddleware` in `src/BuildingBlocks/ControlEasyReborn.Infrastructure/MultiTenancy/`
  - Token storage (frontend): Angular `AuthService` signal; attached by `authInterceptor`
  - Config: `Jwt:SigningKey`, `Jwt:Issuer`, `Jwt:Audience`, `Jwt:AccessTokenTtlMinutes`, `Jwt:RefreshTokenTtlDays` in `appsettings.json` / Docker env

**Bootstrap identity:**
- Platform admin seeded at startup when configured — `src/Host/ControlEasyReborn.Api/Hosting/PlatformAdminBootstrapService.cs`
  - Config: `Bootstrap:PlatformAdminEmail`, `Bootstrap:PlatformAdminPassword` in `appsettings.json`

**OAuth Integrations:**
- Not detected

## Monitoring & Observability

**Error Tracking:**
- None — no Sentry, Application Insights, or similar APM integration

**Logs:**
- Serilog — structured logging configured in `src/Host/ControlEasyReborn.Api/Program.cs`
  - Sinks: Console (JSON) + Seq (`Serilog.Sinks.Seq` 6.0.0)
  - Seq service: `datalust/seq:latest` in `docker/docker-compose.yml`; URL via `Serilog__WriteTo__1__Args__serverUrl` (default `http://seq:5341` in compose, `http://localhost:5341` in `appsettings.json`)
  - Request logging: `UseSerilogRequestLogging()` middleware

**Health:**
- ASP.NET Core health checks — `GET /health` (MySQL connectivity)

## CI/CD & Deployment

**Hosting:**
- Docker Compose — local and demo deployments via `docker/docker-compose.yml` (+ `docker/docker-compose.demo.yml` overlay)
- Traefik v3.1 reverse proxy — TLS-ready entrypoints on ports 8080/8443 (`docker/reverse-proxy/`)
- No cloud hosting configuration (AWS, Azure, GCP, Kubernetes manifests) detected in repository

**CI Pipeline:**
- Not detected — no `.github/workflows/*.yml` or other CI config files present (only `.github/copilot-instructions.md` exists)

## Environment Configuration

**Required env vars (API):**
- `Db__Provider` — e.g. `MySQL`
- `Db__Host`, `Db__Port`, `Db__Database`, `Db__Username`, `Db__Password`
- `Jwt__SigningKey`, `Jwt__Issuer`, `Jwt__Audience`
- `ASPNETCORE_ENVIRONMENT`

**Docker Compose vars (`docker/.env.example`):**
- `MYSQL_ROOT_PASSWORD`, `MYSQL_USER`, `MYSQL_PASSWORD`
- `JWT_SIGNING_KEY`

**Optional / feature flags:**
- `Demo__Enabled`, `Demo__SeedVersion`, `Demo__DisableOutboundEmail`
- `FeatureManagement__Residents__UseWeb`, `FeatureManagement__Visits__UseWeb`, etc.
- `Bootstrap__PlatformAdminEmail`, `Bootstrap__PlatformAdminPassword`
- `Backup__Path`
- `Serilog__WriteTo__1__Args__serverUrl`

**Secrets location:**
- Local/Docker: `docker/.env` (gitignored; template at `docker/.env.example`)
- ASP.NET Core: environment variables and `appsettings.json` (development defaults in repo — replace for production)

**Development mocks/stubs:**
- Testcontainers MySQL for integration tests (no external DB required for `dotnet test`)
- Demo mode overlay seeds sample tenants/residents via `docker/docker-compose.demo.yml` and `src/BuildingBlocks/ControlEasyReborn.Infrastructure/Demo/DemoSeederService.cs`

**Staging/Production:**
- Not configured in repo — deploy via Docker images built from `docker/api.Dockerfile` and `docker/web.Dockerfile`

## Webhooks & Callbacks

**Incoming:**
- None — no webhook receiver endpoints for external services

**Internal admin/demo endpoints (not webhooks):**
- `POST /api/v1/demo/reset` — demo data reset (PlatformAdmin only); defined in `src/BuildingBlocks/ControlEasyReborn.Infrastructure/Demo/DemoEndpoints.cs`
- Tenant backup endpoints — `MapTenantBackupEndpoints()` in `src/Modules/Tenants/ControlEasyReborn.Modules.Tenants.Api/Endpoints/TenantEndpoints.cs`

**Outgoing:**
- None — no outbound HTTP callbacks to external systems detected

## Infrastructure Services (Docker Compose)

| Service | Image | Purpose | Access |
|---------|-------|---------|--------|
| `reverse-proxy` | `traefik:v3.1` | HTTP routing, TLS termination | Host `:8080`, `:8443`, dashboard `:8082` |
| `api` | Built from `docker/api.Dockerfile` | ASP.NET Core 8 API | Internal `:8080`; public `/api` via Traefik |
| `web` | Built from `docker/web.Dockerfile` | Angular SPA (nginx) | Internal `:8080`; public `/` via Traefik |
| `db` | `mysql:8.0` | Primary database | Internal only |
| `adminer` | `adminer:4` | Database admin UI | `/db` via Traefik |
| `seq` | `datalust/seq:latest` | Log aggregation | Internal `:5341` |

**External CLI dependencies (tenant backup):**
- `mysqldump` and `gzip` — invoked by `TenantBackupService`; must be available in the API runtime environment when backup features are used (not included in `docker/api.Dockerfile` today)

---

*Integration audit: 2026-06-24*
*Update when adding/removing external services*
