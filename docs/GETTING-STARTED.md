<!-- generated-by: gsd-doc-writer -->
# Getting Started — ControlEasy Reborn

This guide provides step-by-step instructions for booting up, configuring, and verifying the **ControlEasy Reborn** Docker Compose stack.

## Prerequisites

Before starting, ensure your system has the following installed:

| Requirement | Minimum Version | Notes |
|---|---|---|
| **Docker Engine** | 24.0+ | Container runtime |
| **Docker Compose** | v2.20+ | Multi-container orchestration (`docker compose`) |
| **Git** | 2.30+ | Version control |
| **Free Ports** | — | Ports `8080` (HTTP), `8443` (HTTPS), `8082` (Traefik dashboard) |

## Quick Installation & First Boot

### 1. Clone the Repository
```bash
git clone https://github.com/reisfelipe18/ControlEasy.git
cd ControlEasy
```

### 2. Configure Environment (Optional)
By default, the stack uses development defaults. You can copy the example environment file:
```bash
cp docker/.env.example docker/.env
```
For variable descriptions and secret options, see the [Configuration Reference](CONFIGURATION.md).

### 3. Launch the Stack
Start all services using Docker Compose:
```bash
docker compose -f docker/docker-compose.yml up -d --build
```
Or use the Makefile shortcut:
```bash
make up
```

Docker Compose spins up six containers:
- `reverse-proxy`: Traefik v3.1 gateway (port 8080)
- `api`: ASP.NET Core 8 Web API
- `web`: Angular 18 SPA served via Nginx
- `db`: MySQL 8.0 database
- `adminer`: Lightweight database web interface (path `/db`)
- `seq`: Centralized structured logging instance

---

## What Happens on First Boot

When starting with an empty database volume (`mysql-data`):

1. **MySQL Database Initialization:**
   The `db` container runs scripts from `docker/mysql/init/` in lexical order:
   - `00-schema.sql`: Core users and accounts
   - `02-tenants-seed.sql`: Default tenant creation (`Condominio Padrao`)
   - `02a-residents-schema.sql`: Residents and units tables
   - `03-tenant-backfill.sql`: Multi-tenant key constraints
   - `04-tenant-views.sql`: Tenant-scoped views
   - `05-security-schema.sql`: Gatehouses, shifts, attendant profiles
   - `06-visits-schema.sql`: Visits and visitor records
   - `07-vehicles-schema.sql`: Vehicles and license plates
   - `08-service-providers-schema.sql`: Service providers and contractors
   - `09-administration-schema.sql`: System administration configuration
   - `12-photos-schema.sql`: Photos and consent policy ledger

2. **PlatformAdmin Bootstrap:**
   Once MySQL reports healthy, the `api` container executes `PlatformAdminBootstrapService`. If no platform administrator exists, it generates temporary credentials and outputs them to the container log:

   ```bash
   docker compose -f docker/docker-compose.yml logs api | grep "CHANGE IMMEDIATELY"
   ```

   **Example output:**
   ```text
   [WRN] CHANGE IMMEDIATELY — PlatformAdmin seeded. Email: platform-admin@controleasy.local, Password: ...
   ```

---

## Access URLs

All web traffic is routed through the Traefik reverse proxy on port **8080**:

| Service | Access URL | Credentials / Purpose |
|---|---|---|
| **Web Application** | [http://localhost:8080](http://localhost:8080) | Main Angular portal |
| **REST API / Swagger** | [http://localhost:8080/api](http://localhost:8080/api) | Interactive OpenAPI documentation (dev mode) |
| **Adminer (MySQL UI)** | [http://localhost:8080/db](http://localhost:8080/db) | Server: `db`, User: `controleasy`, Pass: `controleasy_dev`, DB: `controleasydb` |
| **Traefik Dashboard** | [http://localhost:8082](http://localhost:8082) | Reverse proxy status and routing rules |

---

## First Sign-In Workflow

1. Open [http://localhost:8080](http://localhost:8080) in your browser.
2. Enter the generated bootstrap email and password from the API logs.
3. Because the account is seeded with `MustChangePassword = true`, you will immediately be prompted to change your password.
4. Enter the temporary password as your current password, choose a secure new password (min 8 characters), and confirm.
5. Upon saving, you are redirected to the platform dashboard.

---

## Evaluating with Demo Mode

If you prefer exploring the system with rich sample data (two themed condominiums, residents, vehicles, visits, and fixed passwords) without manual entry:

```bash
docker compose -f docker/docker-compose.yml -f docker/docker-compose.demo.yml up -d --build
```

**Fixed Demo Credentials:**
- **Gatehouse Attendant:** `porteiro@controleasy.app` / `demo123`
- **Condominium Admin:** `admin@aurora.controleasy.app` / `demo123`
- **Platform Operator:** `platform@controleasy.app` / `demo123`

For full demo walkthroughs and sample personas, see [Demo Mode](demo-mode.md).

---

## Troubleshooting & Common Setup Issues

### 1. Port Conflict on 8080 or 8082
If port 8080 or 8082 is already bound by another service on your machine:
- Open `docker/.env` (or create one from `docker/.env.example`).
- Adjust `REVERSE_PROXY_PORT=8090` and restart the stack:
  ```bash
  docker compose -f docker/docker-compose.yml up -d
  ```

### 2. Database Volume Reset (Fresh Start)
To completely wipe existing data and rerun all schema and bootstrap scripts:
```bash
docker compose -f docker/docker-compose.yml down -v
docker compose -f docker/docker-compose.yml up -d --build
```
Retrieve the new bootstrap credentials from the logs after restart.

### 3. Database Container Fails to Become Healthy
Inspect the MySQL container logs:
```bash
docker compose -f docker/docker-compose.yml logs db
```
Ensure you have sufficient disk space and that no local MySQL service on port 3306 conflicts with Docker networking.

---

## Next Steps

- Explore the system architecture: [Architecture Overview](ARCHITECTURE.md)
- Learn how to build and test locally: [Development Guide](DEVELOPMENT.md)
- Configure test suites and CI: [Testing Guide](TESTING.md)
- Review available REST endpoints: [API Documentation](API.md)
