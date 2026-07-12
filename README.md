# ControlEasy Reborn

**ControlEasy Reborn** is a condominium access-control web platform
for managing residents, visitors, vehicles, service providers,
apartments, and gatehouse ("portaria") operations.

## Stack

- **Frontend:** Angular 18+ SPA (standalone components, signals, TypeScript strict mode)
- **Backend:** ASP.NET Core 8 (LTS) Web API — modular monolith / Clean Architecture / vertical slices
- **Data access:** [`DBTools_SQL`](https://github.com/gednt/DBTools_SQL) (LINQ-first, multi-provider: MySQL today, PostgreSQL/SQL Server/SQLite ready)
- **Database:** MySQL 8
- **Containerization:** Docker + Docker Compose (api, web, db, reverse-proxy, adminer, seq)
- **Auth:** JWT bearer tokens
- **Type-safety across the wire:** OpenAPI schema from Swashbuckle → Angular types via `ng-openapi-gen`

## Project layout

```
ControlEasy/
├── AGENTS.md                              # Architecture, conventions, stack (authoritative)
├── README.md
├── .planning/                             # GSD roadmap and project-level state
├── .specify/                              # spec-kit configuration and memory
├── .specs/                                # per-feature specs (requirements / design / tasks)
├── openspec/                               # OpenSpec change-tracking (optional)
├── src/                                   # ASP.NET Core 8 API + Angular 18 SPA
│   ├── Host/ControlEasyReborn.Api/
│   ├── Web/ControlEasyReborn.Web/
│   ├── BuildingBlocks/
│   └── Modules/
├── tests/
└── docker/
```

## Workflow tooling

This project runs three workflow systems:

- **GSD** — planner and roadmap owner (`.planning/`)
- **spec-kit** — per-feature specification (`.specs/<feature>/`)
- **OpenSpec** — opt-in change-tracking agreement layer (`openspec/`) — used at user discretion

For details, see [AGENTS.md](AGENTS.md).

## Design System

The visual design system is documented in [`docs/design-system/README.md`](docs/design-system/README.md).

**Showcase:** `/design-system/showcase` (live component gallery, both themes)

## Local development

The canonical dev workflow is documented in [AGENTS.md § 4](AGENTS.md#4-local-development-canonical-devcontainer-mode).

First boot:

```bash
docker compose -f docker/docker-compose.yml up -d --build
```

## Demo mode

Pre-populated stack with fixed credentials and two themed condominiums for sales demos and training:

```bash
docker compose -f docker/docker-compose.yml -f docker/docker-compose.demo.yml up -d --build
```

Sign in with `porteiro@controleasy.app` / `demo123` (or any demo persona). See [`.specs/demo-mode/`](.specs/demo-mode/requirements.md) for credentials and walkthrough scripts.

**Never use the demo JWT signing key or demo overlay in production.**

## Documentation

- **[AGENTS.md](AGENTS.md)** — architecture, conventions, stack, local development workflow (authoritative)
- **[.planning/PROJECT.md](.planning/PROJECT.md)** — project brief and roadmap (GSD)
- **[.specs/](.specs/)** — per-feature specifications (spec-kit)

## License

Internal / proprietary.