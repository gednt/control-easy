# 0001 — Modular Monolith

Date: 2026-06-13

## Status

Accepted

## Context

ControlEasy Reborn must organize a growing set of business domains (Tenants, Residents, Visits, Vehicles, ServiceProviders, etc.) within a single deployable unit. The options for code organization are:

1. **Monolith without module boundaries** — all code in a single project or loosely separated folders.
2. **Modular monolith** — single deployable with strict module boundaries, each module owning its Domain, Application, Infrastructure, and Api layers.
3. **Microservices** — each module deployed as an independent service with its own database.

## Decision

We adopt **modular monolith** (option 2).

Each business domain (Tenants, Residents, Visits, etc.) is a self-contained module under `src/Modules/<Module>/` with four layers:

- **Domain** — pure domain entities, value objects, domain events. No dependencies.
- **Application** — handlers, DTOs, validators, repository interfaces. Depends only on its own Domain.
- **Infrastructure** — repository implementations, DI extensions. Depends on Application and `BuildingBlocks/Infrastructure`.
- **Api** — minimal API endpoints, authorization handlers. Depends on Application and Infrastructure.

Shared cross-cutting concerns live in `BuildingBlocks/` (`SharedKernel`, `Infrastructure`).

The entire solution is a single deployable (`Host/ControlEasyReborn.Api`). No inter-module project references exist; modules communicate only through the shared `BuildingBlocks` and the host wires them together.

## Consequences

- **Pros:** Single deployment simplicity; strict module boundaries prevent big-ball-of-mud; each module can be tested in isolation; straightforward extraction to microservices later (split a module out, give it its own host, add inter-service transport).
- **Cons:** All modules share the same database connection and process; a failure in one module can bring down the whole process; deployment coupling — any change requires full redeployment.
- **Future:** If a module needs independent scaling or deployment, it can be extracted into a separate service. The clean layering and lack of inter-module coupling makes this a mechanical extraction, not a redesign.