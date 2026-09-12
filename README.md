<!-- generated-by: gsd-doc-writer -->
# ControlEasy Reborn

ControlEasy Reborn is a multi-tenant condominium access-control web platform for managing residents, visitors, vehicles, service providers, apartments, photos, consent policies, and gatehouse ("portaria") operations.

## Overview

Built as a high-performance modular monolith with an Angular 18 SPA frontend and an ASP.NET Core 8 minimal API backend, ControlEasy Reborn enforces strict tenant isolation, role- and permission-based authorization, and Docker-driven local and production workflows.

### Technology Stack

- **Backend:** ASP.NET Core 8 (C# 12 minimal API endpoints, Clean Architecture per module)
- **Frontend:** Angular 18 SPA (standalone components, signals, OnPush change detection, Lucide icons)
- **Data Access:** [DBTools](https://www.nuget.org/packages/DBTools) 1.4.3 (`Linq<T>`, `IAsyncSqlClient`) — No EF Core
- **Database:** MySQL 8
- **Reverse Proxy & Gateway:** Traefik v3.1
- **Logging & Diagnostics:** Serilog (Console JSON + Seq)
- **Client Generation:** OpenAPI 3 schema via Swashbuckle → Angular services via `ng-openapi-gen`
- **Testing:** xUnit, FluentAssertions, NSubstitute, Testcontainers.MySql, NetArchTest.Rules, Playwright, Karma/Jasmine

## Prerequisites

- **Docker Engine 24+** with **Docker Compose v2** (`docker compose`)
- **Git**
- Available host ports: `8080` (HTTP), `8443` (HTTPS), `8082` (Traefik dashboard)

## Quick Start

1. **Clone the repository:**
   ```bash
   git clone https://github.com/reisfelipe18/ControlEasy.git
   cd ControlEasy
   ```

2. **Configure environment (optional for local development):**
   ```bash
   cp docker/.env.example docker/.env
   ```

3. **Start the complete stack:**
   ```bash
   docker compose -f docker/docker-compose.yml up -d --build
   ```
   Or using the Makefile shortcut:
   ```bash
   make up
   ```

4. **Retrieve first-boot credentials & sign in:**
   Inspect the API container logs to obtain the generated PlatformAdmin credentials:
   ```bash
   docker compose -f docker/docker-compose.yml logs api | grep "CHANGE IMMEDIATELY"
   ```
   Open [http://localhost:8080](http://localhost:8080) to sign in and set your new password.

## Demo Mode

To evaluate ControlEasy Reborn with pre-populated sample data (two themed condominiums, residents, visitors, attendant profiles, and fixed credentials `demo123`):

```bash
docker compose -f docker/docker-compose.yml -f docker/docker-compose.demo.yml up -d --build
```

Access personas and sample scenarios:
- **Portaria / Attendant:** `porteiro@controleasy.app` / `demo123`
- **Tenant Administrator:** `admin@aurora.controleasy.app` / `demo123`
- **Platform Administrator:** `platform@controleasy.app` / `demo123`

> **Security Warning:** Never use the demo signing key or demo overlay in production environments.

## Repository Layout

```
ControlEasy/
├── src/
│   ├── Host/ControlEasyReborn.Api/             # API entrypoint, DI, hosting, endpoint mapping
│   ├── Web/ControlEasyReborn.Web/              # Angular 18 SPA (standalone components & signals)
│   ├── BuildingBlocks/                         # Shared kernel, data access, auth, middleware
│   └── Modules/                                # Feature modules (Clean Architecture)
│       ├── Administration/                     # Condominium & system administration
│       ├── Apartments/                         # Units, blocks, occupancy
│       ├── Photos/                             # Photo capture, storage, and consent policies
│       ├── Reports/                            # Operational reports & dashboard analytics
│       ├── Residents/                          # Resident profiles, contacts, units
│       ├── Security/                           # Users, authentication, shifts, gatehouses
│       ├── ServiceProviders/                   # Contractors, companies, authorizations
│       ├── Tenants/                            # Multi-tenancy isolation and tenant admins
│       ├── Vehicles/                           # Vehicle registration and license plates
│       └── Visits/                             # Visitor registration, check-in, check-out
├── tests/
│   ├── ControlEasyReborn.UnitTests/            # Domain & Application unit tests
│   ├── ControlEasyReborn.IntegrationTests/     # Database integration tests (Testcontainers)
│   ├── ControlEasyReborn.ArchitectureTests/    # NetArchTest Clean Architecture validation
│   ├── a11y/                                   # Axe core accessibility tests
│   └── visual/                                 # Playwright visual regression tests
├── docker/                                     # Docker compose, Dockerfiles, MySQL init SQL
└── docs/                                       # Project documentation
```

## Documentation Directory

Comprehensive documentation is available in the [`docs/`](docs/) directory:

- [Architecture Guide](docs/ARCHITECTURE.md) — Modular monolith architecture, data flow, key abstractions
- [Getting Started Guide](docs/GETTING-STARTED.md) — First boot, database initialization, access URLs, troubleshooting
- [Configuration Reference](docs/CONFIGURATION.md) — Environment variables, appsettings, secrets, and overrides
- [Development Guide](docs/DEVELOPMENT.md) — Local development workflow, build commands, code standards
- [Testing Guide](docs/TESTING.md) — Test suites, running tests, writing new tests, CI pipeline
- [API Documentation](docs/API.md) — REST API endpoints, authentication, request/response formats
- [Deployment Guide](docs/DEPLOYMENT.md) — Docker container deployment, CI/CD pipeline, and monitoring
- [Demo Mode Specification](docs/demo-mode.md) — Demo data schemas, credentials, walkthroughs
- [Agent Flow Cheatsheet](docs/agent-flow-cheatsheet.md) — Multi-agent orchestrations and lifecycle conventions
- [AGENTS.md](AGENTS.md) — Authoritative contributor rules and architectural invariants

## License

Internal / Proprietary. All rights reserved.