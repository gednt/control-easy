# ControlEasy

Modernization of the legacy **ControlEasy 5** (WPF / .NET Framework 4.8 / MySQL) condominium access-control desktop application.

This repository contains only the **new** stack:

- **Frontend:** Angular 18+ SPA (standalone components, signals, TypeScript strict mode)
- **Backend:** ASP.NET Core 8 (LTS) Web API — modular monolith / Clean Architecture / vertical slices
- **Data access:** [`DBTools_SQL`](https://github.com/gednt/DBTools_SQL) (LINQ-first, multi-provider: MySQL today, PostgreSQL/SQL Server/SQLite ready)
- **Database:** MySQL 8
- **Containerization:** Docker + Docker Compose (api, web, db, reverse-proxy, adminer, seq)
- **Auth:** JWT bearer tokens
- **Type-safety across the wire:** OpenAPI schema from Swashbuckle → Angular types via `ng-openapi-gen`

The legacy WPF source is **not** tracked here; it lives in a separate reference repository (`ControlEasyReborn/`) and is migrated into this one screen by screen using the Strangler Fig pattern.

## Project layout

```
ControlEasy/
├── AGENTS.md                              # Architecture, conventions, stack
├── README.md
├── .gitignore
├── .specs/
│   └── modernization-roadmap/             # The modernization spec (this is the contract)
│       ├── requirements.md
│       ├── design.md
│       └── tasks.md
├── src/                                   # Created in Phase 1 — greenfield code only
│   ├── Host/ControlEasyReborn.Api/        # ASP.NET Core 8 API
│   ├── Web/ControlEasyReborn.Web/         # Angular 18+ SPA
│   ├── BuildingBlocks/                    # SharedKernel, Infrastructure, Contracts
│   └── Modules/                           # Residents, Visits, Vehicles, ServiceProviders, Security, Administration
├── tests/                                 # Created in Phase 1
└── docker/                                # Created in Phase 1 — api.Dockerfile, web.Dockerfile, docker-compose.yml
```

The modernization is governed by `.specs/modernization-roadmap/` — see `requirements.md` for the user stories, `design.md` for the architecture, and `tasks.md` for the 4-phase execution plan with verification gates.

## Status

**Phase 0 — Inventory & Cleanup** (planning).
The first greenfield commit lands in **Phase 1** of `tasks.md`. The legacy code stays in the reference repository until each module is migrated via the Strangler pattern.

## Local development (target)

```bash
docker compose -f docker/docker-compose.yml up -d
# api      → http://localhost:8080/api
# web      → http://localhost:8080
# adminer  → http://localhost:8080/db
# seq      → http://localhost:5341
```

## Demo mode

Pre-populated stack with fixed credentials and two themed condominiums for sales demos and training:

```bash
docker compose -f docker/docker-compose.yml -f docker/docker-compose.demo.yml up -d --build
```

Sign in with `porteiro@controleasy.app` / `demo123` (or any demo persona). See [docs/demo-mode.md](docs/demo-mode.md) for the full credential table, walkthrough script, and reset procedures.

**Never use the demo JWT signing key or demo overlay in production.**

## License

Internal / proprietary.
