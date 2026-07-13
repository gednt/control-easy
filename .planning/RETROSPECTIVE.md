# Project Retrospective

## Milestone: v1.0 — Reborn MVP

**Shipped:** 2026-07-13  
**Phases:** 11 | **Plans:** 11

### What Was Built

- Tenant-isolated ASP.NET Core modular monolith with the core condominium access-control modules.
- Angular SPA with authenticated tenant workflows and a hardened reusable design system.
- Canonical Docker Compose runtime plus deterministic opt-in demo mode.
- DBTools 1.4.3 NuGet persistence, runtime health checks, CI, OpenAPI generation, and architecture contracts.

### What Worked

- Cross-referencing implementation, specs, tests, and live containers exposed stale planning state without duplicating shipped work.
- Tenant-isolation regression tests and architecture guards converted critical invariants into executable checks.
- Rebuilding API and web images after changes caught runtime issues that unit-only verification would have missed.

### What Was Inefficient

- Much of the shipped implementation predated GSD evidence, requiring retrospective plan, summary, verification, and validation artifacts.
- Spec task checkboxes and roadmap counts drifted from the repository, obscuring the true remaining scope.
- Generated OpenAPI output lacks rich response typing and is not yet adopted by handwritten Angular services.

### Patterns Established

- Active milestone scope is counted separately from explicitly deferred v1.1/v2 requirements.
- Every tenant-sensitive data path receives both integration coverage and an architecture-level guard where practical.
- Release verification includes .NET, Angular, accessibility/visual checks, Docker rebuild, live health, and demo isolation.

### Key Lessons

- Keep GSD evidence and spec checkboxes synchronized when implementation lands, not at milestone close.
- Treat generated contracts as useful only when endpoint metadata and application consumption are both verified.
- Milestone audits should distinguish missing evidence from missing implementation before creating closure work.

## Cross-Milestone Trends

| Milestone | Requirements | Verification | Main debt |
|---|---:|---|---|
| v1.0 | 32/32 active | 79 unit, 55 integration, 6 architecture, 203 Angular | Generated-client adoption |
