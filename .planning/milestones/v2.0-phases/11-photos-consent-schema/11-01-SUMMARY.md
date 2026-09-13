---
phase: 11
plan: 1
subsystem: photos + consent
tags: [photos, consent, storage, schema, audit-log, append-only]
requires:
  - PHOTO-01
  - CONSENT-01
  - CONSENT-02
provides:
  - IStorageProvider abstraction (Local + S3/MinIO + Amazon S3)
  - photos table + soft-delete
  - consent_audit_log table (append-only)
  - tenant_consent_policy table
  - POST /api/v1/photos (photos.write)
  - GET /api/v1/photos/{id} (photos.read)
  - DELETE /api/v1/photos/{id} soft-delete (photos.delete)
  - POST /api/v1/entry-log (consent-validated)
  - GET /api/v1/entry-log (filtered list)
  - GET /api/v1/entry-log/export (CSV)
  - GET /api/v1/consent-policy
  - PUT /api/v1/consent-policy/{tenantId} (tenant admin)
  - Append-only DB triggers on consent_audit_log
  - CHECK constraint: entered_with_consent requires photo_id
  - Photos.Read / Photos.Write / Photos.Delete permissions
  - 118 unit + 73 integration tests
  - Photos module under Modules/Photos with Clean Architecture
affects:
  - Residents (future photo attachments)
  - Visits (future photo attachments)
  - Vehicles (future photo attachments)
  - ServiceProviders (future photo attachments)
  - Tenants (consent policy config)
  - Reports (audit log review)
tech-stack:
  added:
    - DBTools 1.4.3 (already in repo)
    - AWSSDK.S3 (for AmazonS3StorageProvider)
    - MinIO client (for S3StorageProvider)
key-files:
  created:
    - src/Modules/Photos/Photos.Domain/Entities/Photo.cs
    - src/Modules/Photos/Photos.Domain/Entities/ConsentAuditLog.cs
    - src/Modules/Photos/Photos.Domain/Entities/TenantConsentPolicy.cs
    - src/Modules/Photos/Photos.Domain/Enums/EntryState.cs
    - src/Modules/Photos/Photos.Domain/Enums/OverrideReason.cs
    - src/Modules/Photos/Photos.Application/Storage/IStorageProvider.cs
    - src/Modules/Photos/Photos.Application/Handlers/UploadPhotoHandler.cs
    - src/Modules/Photos/Photos.Application/Handlers/GetPhotoHandler.cs
    - src/Modules/Photos/Photos.Application/Handlers/SoftDeletePhotoHandler.cs
    - src/Modules/Photos/Photos.Application/Handlers/CreateEntryLogHandler.cs
    - src/Modules/Photos/Photos.Application/Handlers/ListEntryLogsHandler.cs
    - src/Modules/Photos/Photos.Application/Handlers/ExportEntryLogCsvHandler.cs
    - src/Modules/Photos/Photos.Application/Handlers/UpdateConsentPolicyHandler.cs
    - src/Modules/Photos/Photos.Infrastructure/Storage/LocalFilesystemStorageProvider.cs
    - src/Modules/Photos/Photos.Infrastructure/Storage/S3StorageProvider.cs
    - src/Modules/Photos/Photos.Infrastructure/Storage/AmazonS3StorageProvider.cs
    - src/Modules/Photos/Photos.Api/PhotosEndpoints.cs
    - src/Modules/Photos/Photos.Api/EntryLogEndpoints.cs
    - src/Modules/Photos/Photos.Api/ConsentPolicyEndpoints.cs
    - src/Host/ControlEasyReborn.Api/Migrations/*-photos-consent.sql
  modified:
    - src/Directory.Packages.props (added AWSSDK.S3)
    - src/ControlEasyReborn.sln (added Photos module projects)
    - src/BuildingBlocks/Security/Permissions.cs (added Photos.* permissions)
    - src/Modules/Tenants/Tenants.Infrastructure/Seeders/TenantSeeder.cs (default consent policy)
    - docker/docker-compose.yml (MinIO service)
    - docker/docker-compose.demo.yml (MinIO seed data)
decisions:
  - Append-only audit log enforced via DB triggers (not application constraint) — defense in depth
  - CHECK constraint enforces entered_with_consent → photo_id NOT NULL at the schema level
  - Storage abstraction: three providers (local dev, MinIO-compatible S3, native Amazon S3) — pick at startup via STORAGE__PROVIDER
  - Photo processing deferred to client (Phase 12) — server only stores what client uploads
  - Phase 11/13 boundary collapsed: shipped in one commit because backend is interdependent
---

# Phase 11 Summary — Photos & Consent Schema Infrastructure

## Result

✅ **Shipped** — commit `d895c01`, 2026-09-12

## What was delivered

### Storage abstraction
- `IStorageProvider` interface — `UploadAsync`, `DownloadAsync`, `DeleteAsync`, `GetThumbnailAsync`
- `LocalFilesystemStorageProvider` (dev): `./storage/photos/{tenantId}/{photoId}.{ext}`
- `S3StorageProvider` (production, MinIO-compatible): uses MinIO endpoint for S3-compatible API
- `AmazonS3StorageProvider` (production, native AWS S3): uses AWSSDK.S3 directly
- Configuration via env vars: `STORAGE__PROVIDER`, `STORAGE__LOCAL__PATH`, `STORAGE__S3__*`

### Schema
- `photos` table: id, tenant_id, entity_type (resident/visitor/vehicle/service-provider), entity_id, file_path, thumbnail_path, file_size_bytes, mime_type, captured_at, uploaded_by, created_at, deleted_at
- `consent_audit_log` table: id, tenant_id, entry_state (entered_with_consent / entered_override / gatehouse_only / denied / entered_without_consent), entity_type, entity_id, photo_id (nullable), override_reason (nullable, emergency/vouched), porteiro_id, recorded_at (datetime3, ms precision), created_at
- `tenant_consent_policy` table: tenant_id, category (dwellers/visitors/service-providers/vehicles), photo_required (bool)
- DB triggers: append-only enforcement on consent_audit_log (rejects UPDATE, DELETE)
- CHECK constraint: entered_with_consent → photo_id NOT NULL

### Endpoints (Photos module)
- `POST /api/v1/photos` (photos.write) — upload
- `GET /api/v1/photos/{id}` (photos.read) — retrieve source
- `GET /api/v1/photos/{id}/thumbnail` (photos.read) — retrieve 128×128 thumbnail
- `DELETE /api/v1/photos/{id}` (photos.delete) — soft-delete

### Endpoints (Consent module)
- `POST /api/v1/entry-log` — create entry (validates consent policy, enforces state transitions, creates audit log entry)
- `GET /api/v1/entry-log?entryState=...&subjectType=...&fromUtc=...&toUtc=...` — list with filters
- `GET /api/v1/entry-log/export?...` — CSV export with millisecond-precision recorded_at
- `GET /api/v1/consent-policy/{tenantId}` — get policy
- `PUT /api/v1/consent-policy/{tenantId}` (tenant admin) — update per-category photo_required toggles

### Permissions
- `photos.read`, `photos.write`, `photos.delete`
- `consent.read`, `consent.write` (for policy)
- `entry-log.create`, `entry-log.read`, `entry-log.export`
- Added to Security module defaults; tenant seed grants defaults

### Tests
- 118 unit tests (handler logic, validation, state transitions, storage mocks)
- 73 integration tests (Testcontainers.MySql):
  - Cross-tenant isolation (photo + entry log)
  - Append-only triggers (UPDATE/DELETE rejected)
  - CHECK constraint (entered_with_consent without photo_id → SQL error)
  - Policy enforcement (photo_required=true → entry without photo rejected)
  - CSV export formatting (millisecond timestamps, escape rules)
  - Storage provider round-trip (local, MinIO)
  - Permission gates (403 on missing scope)

## Deviations from ROADMAP

- **Phase 11 and Phase 13 backend shipped together** — implementation collapsed the 11/13 boundary because the photo backend (Phase 11) and the consent entry-log backend (Phase 13) share the `consent_audit_log` table and validation paths. Shipped in single commit `d895c01`.
- **Five entry states instead of four** — code includes `entered_without_consent` in addition to the four states specified in the ROADMAP (entered_with_consent, entered_override, gatehouse_only, denied). Added as a defensive state for entries logged without explicit consent flag; rejected by CHECK if category policy requires photo.

## Out of scope (deferred)

- UI for photo capture (Phase 12) — not started
- UI for gatehouse workflow / audit review (Phase 13 UI) — not started
- Door integration (Phase 14/15, v2.1) — gated on hardware

## Acceptance evidence

- `d895c01` — feat(photos): implement Photos & Consent Schema Infrastructure
- `.planning/milestones/v2-ROADMAP.md` Phase 11 success criteria all ✅
- All 6 success criteria from ROADMAP met
- `dotnet test` green (unit + integration + architecture)
- Docker stack healthy with MinIO service running
