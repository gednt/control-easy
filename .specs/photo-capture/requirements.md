# Requirements — Photo Capture & Storage

> Spec folder: `.specs/photo-capture/`
> Milestone: v2.0 — Gatehouse Photo & Consent Ledger
> Phases: 11 (schema infrastructure), 12 (capture & display)
> Supersedes: `.specs/_retired/3 - photo-capture-hardware-integration/` (storage + capture subsets)

## Context

The porteiro needs to photograph residents, visitors, vehicles, and service providers at the gatehouse so the photo is on the record for identity verification. The camera is the browser's (`getUserMedia`); the storage is a pluggable provider (local filesystem for dev, S3/MinIO for production). All image processing is client-side — no server-side compression, thumbnailing, or EXIF stripping.

## User Stories

### Photo Storage & Retrieval

- **UC-PC-01:** As a *gatehouse attendant (porteiro)*, I want to capture a photograph of a visitor, resident, service provider, or vehicle at check-in so the photo is stored and retrievable for identity verification at the gatehouse.
- **UC-PC-02:** As a *tenant administrator*, I want to view and manage all photos associated with residents, visitors, vehicles, and service providers in my condominium, including soft-deleting photos that are no longer needed.
- **UC-PC-03:** As a *platform architect*, I want all photos stored behind an abstraction layer (`IStorageProvider`) with swappable backends (local filesystem for dev, MinIO/S3 for production) so the storage implementation can change without modifying business logic.
- **UC-PC-04:** As a *security officer*, I want photo access logged in the audit trail and restricted by permission (`photos.read`, `photos.write`, `photos.delete`) so only authorized users can view or modify sensitive images.
- **UC-PC-05:** As a *resident (morador)*, I want to upload or update my own profile photo from the web UI so my identity is visible to gatehouse attendants during verification.

### Photo Capture & Compression

- **UC-PC-06:** As a *gatehouse attendant*, I want to capture a photo using the gatehouse workstation's camera directly from the browser, without installing any software, so the capture workflow is instant and hardware-free.
- **UC-PC-07:** As a *gatehouse attendant*, I want to upload an existing image file (from a phone, camera, or shared folder) as an alternative to live camera capture, so I can handle cases where the camera is unavailable or the photo was taken elsewhere.
- **UC-PC-08:** As a *developer*, I want photos resized and compressed client-side (longest dimension ≤1280px, ≤500KB JPEG) before upload so the server receives a predictable, lightweight payload and gatehouse WiFi uploads are fast.
- **UC-PC-09:** As a *security officer*, I want EXIF metadata stripped from all uploaded photos so the condominium's GPS coordinates, camera models, and capture timestamps are never leaked.
- **UC-PC-10:** As a *developer*, I want a 128×128 thumbnail generated client-side alongside the source image so the table views load instantly without server-side processing on first request.
- **UC-PC-11:** As a *gatehouse attendant*, I want upload failures to retry automatically (3 attempts with backoff) so a momentary WiFi drop doesn't lose the photo I just captured.
- **UC-PC-12:** As a *gatehouse attendant*, I want clear error messages when capture or upload fails (camera denied, file too large, unsupported type, all retries exhausted) so I know what to do next.

## Requirements

### PHOTO-01: Storage Infrastructure & API

- `IStorageProvider` abstraction with `LocalFilesystemStorageProvider` (dev) and `S3StorageProvider` (production, MinIO-compatible)
- `photos` table: `id`, `tenant_id`, `entity_type`, `entity_id`, `file_path`, `thumbnail_path`, `file_size_bytes`, `mime_type`, `captured_at`, `uploaded_by`, `created_at`, `deleted_at`
- Permissions: `photos.read`, `photos.write`, `photos.delete`
- Endpoints: `POST /api/v1/photos` (upload), `GET /api/v1/photos/{id}` (retrieve), `DELETE /api/v1/photos/{id}` (soft-delete)
- Storage config via env vars: `STORAGE__PROVIDER`, `STORAGE__LOCAL__PATH`, `STORAGE__S3__ENDPOINT`, `STORAGE__S3__BUCKET`, `STORAGE__S3__ACCESSKEY`, `STORAGE__S3__SECRETKEY`

### PHOTO-02: Browser Capture & Compression

- `ce-photo-capture` component: camera mode (`getUserMedia`), upload mode (file picker + drag/drop)
- Client-side compression: resize longest dimension to ≤1280px → `canvas.toBlob('image/jpeg', 0.8)` → size check → retry at 0.6 if >500KB → floor at quality 0.3
- EXIF strip via canvas redraw (verify no GPS, camera model, or timestamp survives)
- 128×128 thumbnail generation client-side
- Upload with retry: 3 attempts, exponential backoff (1s, 2s, 4s)
- File-type validation: `image/jpeg`, `image/png`, `image/heic`; reject others
- Error states: camera permission denied, upload failed after retry, file too large after compression floor
- `ce-photo` display component: thumbnail in table rows (lazy-loaded), source in detail view, click-to-enlarge lightbox
- Integration into resident, visitor, vehicle, service-provider record pages
- Multiple photos per entity (gallery — most recent as primary thumbnail)

## Out of Scope

- Server-side image processing (compression, thumbnailing, EXIF stripping) — all client-side
- AI-powered facial recognition or automated matching — future spec
- Automatic license plate recognition (ALPR) — future spec
- Video recording and storage — future spec
- Hardware camera integration (dedicated cameras, NVR integration) — the camera is the browser's
- Biometric data encryption (biometric templates) — no biometrics in v2.0

## Traceability

| Requirement | Phase | Spec |
|---|---|---|
| PHOTO-01 | Phase 11 | `.specs/photo-capture/` |
| PHOTO-02 | Phase 12 | `.specs/photo-capture/` |

---
*Requirements defined: 2026-08-23*
*Derived from: party-mode v2.0 milestone design session*