# Changelog — ControlEasy Reborn

All notable changes to the **ControlEasy Reborn** web platform are documented in this file.

This project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

> 📜 **Classic Desktop History:** For the chronological release notes and development log of the original desktop application from 2014 (v0.1 through v3.6.0.6), see [`docs/HISTORICAL-CHANGELOG.md`](docs/HISTORICAL-CHANGELOG.md).

---

## [Unreleased]

### Planned (v2.1)
- **Door & Relay Integration:** Optional physical door relay controller (`IDoorController`), signed unlock endpoints, and hardware event queue (gated on condominium hardware adoption).

---

## [2.0.0] - 2026-09-13 — Gatehouse Photo & Consent Ledger

### Added
- **Photo Capture & Storage Architecture (`PHOTO-01`):**
  - Multi-provider photo storage abstraction (`IPhotoStorageService` / `IStorageProvider`) with `LocalFilesystemStorageProvider` for development and `S3StorageProvider` for MinIO/S3 production deployments.
  - Dedicated `photos` database schema with tenant isolation, soft-deletion (`deleted_at`), and upload attribution.
  - Endpoints for photo upload, retrieval, and soft-delete with permission gates (`photos.read`, `photos.write`, `photos.delete`).
- **Browser-Based Photo Capture (`PHOTO-02`):**
  - Standalone `ce-photo-capture` component supporting live webcam capture via `getUserMedia` and drag-and-drop file upload.
  - Client-side image processing pipeline: automatic canvas redraw for instant EXIF metadata stripping (removing camera and GPS tags).
  - Client-side compression scaling images to $\le 1280$px and $\le 500$KB JPEG, with concurrent generation of $128 \times 128$ thumbnails.
  - Automatic upload retry with exponential backoff (3 attempts).
  - Standalone `ce-photo` and gallery components with lightbox view integrated into Residents, Visitors, Vehicles, and Service Providers.
- **Tenant Consent Policies (`CONSENT-01`):**
  - Per-tenant, per-category photo consent policy configuration (`tenant_consent_policy`) for dwellers, visitors, contractors, and vehicles.
  - Configuration UI enabling building managers to toggle photo requirements per entity category.
- **Gatehouse Entry Workflow & Audit Ledger (`CONSENT-02`, `CONSENT-03`):**
  - High-speed 3-second gatehouse entry workflow supporting four formal entry states: `entered_with_consent`, `entered_override`, `gatehouse_only`, and `denied`.
  - Append-only `consent_audit_log` with millisecond-precision timestamps (`datetime3`) designed to cross-reference with external CCTV footage.
  - Database-level constraint guaranteeing that consent entries require a non-null `photo_id`.
  - Override reason code capture (`emergency`, `vouched`) with operator audit logging.
  - Audit log review screen with category, status, date, and attendant filtering, plus millisecond-accurate CSV export.

---

## [1.1.0] - 2026-08-15 — UI Parity & Dashboard Live Statistics

### Added
- **Dashboard Live Analytics (`DASH-01`, `DASH-02`):**
  - `GET /api/v1/dashboard/stats` tenant-scoped endpoint returning live statistics for active residents, active vehicles, occupied apartments, and open visits.
  - Interactive dashboard UI with real-time stat tiles, quick-action shortcuts, and recent shift logs.
- **Vehicle Management Polish (`DASH-04`):**
  - Full vehicle edit and deactivation modal workflows in the web UI backed by `Vehicles.Write` permissions.
- **UI Enhancements & Bug Fixes (`UI-03`, `UI-04`):**
  - Unified icon system powered by Lucide Angular (`ce-icon`).
  - Mobile drawer navigation with responsive backdrop toggle below 640px.
  - Client-side search filtering, status tabs (All, Active, Pending, Overdue), and dynamic pagination across entity tables.
  - Standardized modal and dropdown keyboard interactions (Escape key dismissal, outside click, focus trapping).

---

## [1.0.0] - 2026-06-24 — Modular Monolith MVP

### Added
- **Modular Monolith Architecture:**
  - Modern ASP.NET Core 8 minimal API platform divided into 9 bounded contexts: `Administration`, `Apartments`, `Photos`, `Reports`, `Residents`, `Security`, `ServiceProviders`, `Tenants`, `Vehicles`, and `Visits`.
  - Clean Architecture separation per module (`Domain`, `Application`, `Infrastructure`, `Api`).
  - Strict architectural boundary enforcement via automated `NetArchTest.Rules`.
- **Data Access & Multi-Tenancy:**
  - Integrated [DBTools 1.4.3](https://www.nuget.org/packages/DBTools) (`Linq<TModel>`, `IAsyncSqlClient`) for high-throughput, predictable LINQ-to-SQL data access without Entity Framework Core overhead.
  - Tenant isolation enforced at the query factory level (`ITenantAwareLinqFactory`), preventing cross-tenant data leakage by design.
- **Authentication & Authorization:**
  - JWT Bearer authentication with token refresh lifecycle.
  - Fine-grained role- and permission-based authorization policies (`PlatformAdminRequirement`, `RequirePermissionRequirement`).
  - BCrypt password hashing (cost factor $\ge 11$).
  - Multi-tenancy resolution middleware supporting tenant extraction from JWT claims and `X-Tenant-ID` headers.
- **Angular 18 Frontend:**
  - Built with Angular 18 standalone components, reactive Signals, and `OnPush` change detection.
  - Custom design system using Tailwind CSS v4 and CSS design tokens with light/dark/system theme support.
  - 17 base UI components (`ce-` prefix): button, card, input, stat-tile, badge, modal, toast, table, empty-state, spinner, avatar, tabs, dropdown, tooltip, pagination, breadcrumbs, checkbox.
  - Strongly-typed API client generation from OpenAPI 3 specs via `ng-openapi-gen` with pre-build contract validation (`openapi-check`).
- **Comprehensive Testing Suite:**
  - Unit tests with xUnit, FluentAssertions, and NSubstitute.
  - Database integration tests using real disposable MySQL 8 Docker containers via `Testcontainers.MySql`.
  - End-to-end testing with Playwright and accessibility testing with Axe Core.
- **Deployment & Developer Experience:**
  - Complete Docker Compose stack (`docker compose`) running Traefik v3.1, ASP.NET Core 8 API, Angular 18 Web SPA, MySQL 8, Seq structured logging, and Adminer.
  - Demo Mode overlay (`docker-compose.demo.yml`) offering seeded multi-tenant condominiums and pre-configured test personas (`demo123`).
  - Makefile convenience targets (`make up`, `make down`, `make test`, `make build`).
