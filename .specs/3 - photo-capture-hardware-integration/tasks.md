# Tasks — Photo Capture & Hardware Integration

> Companion to `requirements.md` and `design.md`. Each task is a single, verifiable unit of work. **Phases must be completed in order**; a phase gate (last task of the phase) must be green before starting the next phase.
>
> This spec is additive to `.specs/1 - modernization-roadmap/tasks.md`. Task numbering continues from where the modernization roadmap leaves off (Phase 4 of the roadmap). The tasks below are numbered P5.x (Phase 5 of the overall project).

---

## Phase 5 — Photos Module (backend + frontend)

*Goal: Implement the Photos module end-to-end: storage abstraction, upload pipeline, thumbnail generation, API endpoints, Angular photo capture and gallery components.*

- [ ] **5.1** **Photos Domain layer.** Create `Modules/Photos/ControlEasyReborn.Modules.Photos.Domain/` with entities (`Photo`, `Thumbnail`), value objects (`PhotoId`, `PhotoMetadata`, `CaptureContext`, `ThumbnailSize`), enums (`PhotoEntityType`), and domain events (`PhotoUploaded`, `PhotoDeleted`). All entities carry `TenantId`. Write unit tests confirming entity invariants (e.g., `Photo.FileHash` is SHA-256 length, `Thumbnail.SizeLabel` is one of `small|medium|large`).
  - **Verification:** `dotnet test` passes; `Photo` and `Thumbnail` entities have non-nullable `TenantId` (C.6 rule).

- [ ] **5.2** **Photos Application layer.** Create `Modules/Photos/ControlEasyReborn.Modules.Photos.Application/` with repository interfaces (`IPhotoRepository`, `IThumbnailRepository`), storage abstraction (`IStorageProvider`), thumbnail service (`IThumbnailService`), metadata extractor (`IPhotoMetadataExtractor`), command/query DTOs, handlers (`UploadPhotoCommandHandler`, `DeletePhotoCommandHandler`, `GetPhotoQueryHandler`, `GetPhotosByEntityQueryHandler`, `GetThumbnailQueryHandler`), validators, permissions (`PhotoPermissions`), and Mapster mapping profiles.
  - **Verification:** Project compiles; all handlers reference abstractions only (no `Infrastructure` dependency).

- [ ] **5.3** **Photos Infrastructure — `IStorageProvider` implementations.** Create `LocalFileStorageProvider` (dev) and `MinioStorageProvider` (prod). Both implement `StoreAsync`, `RetrieveAsync`, `DeleteAsync`, `GetPresignedUrlAsync`. DI registration swaps based on `Storage:Provider` config key. Add `MinioStorageOptions` and `LocalFileStorageOptions` config classes. Add Minio NuGet package to CPM (`Directory.Packages.props`).
  - **Verification:** Unit test creates a `LocalFileStorageProvider`, stores a stream, retrieves it, and asserts content equality. Integration test (Testcontainers) verifies `MinioStorageProvider` against a real MinIO container.

- [ ] **5.4** **Photos Infrastructure — thumbnail generation.** Implement `ImageSharpThumbnailService` using SixLabors.ImageSharp. Generates small (128x128), medium (512x512), and large (1024x1024) thumbnails. Stores each thumbnail via `IStorageProvider` under `{originalKey}/thumbnails/{size}.jpg`. Add `SixLabors.ImageSharp` to CPM.
  - **Verification:** Unit test generates thumbnails from a test JPEG and asserts dimensions (128x128, 512x512, 1024x1024) and content type (`image/jpeg`).

- [ ] **5.5** **Photos Infrastructure — EXIF metadata extraction.** Implement `ExifMetadataExtractor` using the `MetadataExtractor` NuGet package. Extracts latitude, longitude, `DateTimeOriginal`, make, model, orientation. Returns `PhotoMetadata` record.
  - **Verification:** Unit test reads a known JPEG with EXIF data and asserts extracted fields match expected values.

- [ ] **5.6** **Photos Infrastructure — repository implementation.** Implement `PhotoRepository` and `ThumbnailRepository` using `Linq<Photo>` / `Linq<Thumbnail>` + `TenantAwareLinqFactory`. Every query includes `TenantId` filter. Implement soft-delete (`IsActive = false`) and permanent-delete (for LGPD biometric cascade).
  - **Verification:** Integration test (Testcontainers + MySQL) creates a photo as tenant A, lists photos as tenant A (sees it), lists photos as tenant B (sees 0), and gets 404 on `FindAsync(id)` as tenant B.

- [ ] **5.7** **Photos API endpoints.** Create `Modules/Photos/ControlEasyReborn.Modules.Photos.Api/` with Minimal API endpoints: `POST /api/v1/photos/upload` (multipart), `GET /api/v1/photos/{id}`, `GET /api/v1/photos/{id}/file`, `GET /api/v1/photos/{id}/thumbnails/{size}`, `DELETE /api/v1/photos/{id}`, `GET /api/v1/residents/{id}/photos`, `GET /api/v1/visits/{id}/photos`, `GET /api/v1/vehicles/{id}/photos`, `GET /api/v1/service-providers/{id}/photos`, `POST /api/v1/residents/{id}/profile-photo`. All endpoints enforce `RequirePermission("photos.read|write|delete")`. Register in `Host/Program.cs` via `AddPhotosModule()`.
  - **Verification:** Integration test uploads a photo, retrieves it, gets thumbnails, soft-deletes it, and asserts each response code. Cross-tenant access test: tenant A uploads a photo, tenant B gets 404.

- [ ] **5.8** **Photos database schema.** Add `docker/mysql/init/05-photos-and-devices-schema.sql` with `Photos` and `Thumbnails` tables (see design.md). Add the tables to the tenant backfill script. Add NetArchTest rule ensuring `Photos` and `Thumbnails` tables are listed.
  - **Verification:** `docker compose down -v && docker compose up -d` — MySQL init succeeds; `Photos` and `Thumbnails` tables exist with `tenant_id` column.

- [ ] **5.9** **Photos Angular module.** Create `PhotosModule` (lazy-loaded at `/photos`) with:
  - `PhotoUploadComponent` (drag-and-drop + file picker, progress bar, calls `POST /api/v1/photos/upload`)
  - `PhotoGalleryComponent` (carousel of photos for an entity, lazy-loaded thumbnails)
  - `PhotoViewerComponent` (click-to-expand full-size via presigned URL)
  - `PhotoDeleteDialogComponent` (confirmation, soft-delete warning)
  - `ProfilePhotoComponent` (resident self-upload, calls `POST /api/v1/residents/{id}/profile-photo`)
  - `CameraCaptureComponent` (uses `navigator.mediaDevices.getUserMedia` for live webcam capture at gatehouse, canvas snapshot, upload)
  - `PhotoService` (Angular service wrapping `HttpClient` for all `/api/v1/photos/*` endpoints)
  - TypeScript DTOs generated via `ng-openapi-gen` from the updated OpenAPI spec
  - Permission guard: `CanActivate` guard checking `photos.read` / `photos.write` / `photos.delete`
  - **Verification:** Open `https://localhost/photos` in Chrome via Playwright; upload a photo via the file picker; see it in the gallery; click to expand; delete it; confirm deletion dialog appears.

- [ ] **5.10** **MinIO + Photos integration test.** Add Testcontainers-based integration test that boots MinIO + MySQL + API, uploads a photo via the API endpoint, verifies the object exists in MinIO, retrieves the photo binary, and deletes it.
  - **Verification:** `dotnet test` — integration test passes with MinIO container.

- **Verification gate (Phase 5):**
  - `docker compose up -d` brings all services healthy (including `minio`).
  - Upload a photo via `POST /api/v1/photos/upload` → 201 Created with presigned URL.
  - Retrieve photo metadata via `GET /api/v1/photos/{id}` → 200 OK with `PhotoDto`.
  - Retrieve photo binary via `GET /api/v1/photos/{id}/file` → 200 OK with `image/jpeg`.
  - Retrieve thumbnail via `GET /api/v1/photos/{id}/thumbnails/small` → 200 OK with `image/jpeg` (128x128).
  - Cross-tenant test: tenant A photo is invisible to tenant B (404).
  - MinIO bucket contains the uploaded object and three thumbnail objects.
  - Angular photo gallery page renders in Chrome; upload, view, delete flows work end-to-end.

---

## Phase 6 — HardwareIntegration Module (backend + MQTT + frontend)

*Goal: Implement the HardwareIntegration module: device registry, MQTT event ingestion, biometric encryption, device health monitoring, camera stream proxy, Angular device dashboard.*

- [ ] **6.1** **HardwareIntegration Domain layer.** Create `Modules/HardwareIntegration/ControlEasyReborn.Modules.HardwareIntegration.Domain/` with entities (`Device`, `DeviceEvent`, `BiometricTemplate`), value objects (`DeviceId`, `EventPayload`), enums (`DeviceType`, `DeviceStatus`), and domain events (`DeviceRegistered`, `DeviceWentOffline`, `BiometricMatchOccurred`). All entities carry `TenantId`. Write unit tests confirming `DeviceType` enum values and `DeviceStatus` transitions.
  - **Verification:** `dotnet test` passes; `Device` and `DeviceEvent` entities have non-nullable `TenantId` (C.6 rule).

- [ ] **6.2** **HardwareIntegration Application layer.** Create `Modules/HardwareIntegration/ControlEasyReborn.Modules.HardwareIntegration.Application/` with repository interfaces (`IDeviceRepository`, `IDeviceEventRepository`, `IBiometricTemplateRepository`), service interfaces (`IBiometricService`, `IDeviceHealthMonitor`, `IDeviceHandler`, `IDeviceEventPublisher`), command/query DTOs, handlers, validators, permissions (`HardwareIntegrationPermissions`), and Mapster mapping profiles. Implement `IDeviceHandler` strategy pattern with stubs for `CameraDeviceHandler`, `BiometricReaderDeviceHandler`, `IntercomDeviceHandler`, `BarrierGateDeviceHandler`, `UnknownDeviceHandler`.
  - **Verification:** Project compiles; all handlers implement `IDeviceHandler`; `SupportedDeviceType` returns the correct enum value.

- [ ] **6.3** **MQTT client background service.** Implement `MqttClientService : BackgroundService` using MQTTnet. Connect to Mosquitto on startup. Subscribe to `controleasy/+/devices/+/events/#` and `controleasy/+/devices/+/heartbeat`. Parse tenant ID and device ID from topic segments. Forward event payloads to `IDeviceEventConsumer.IngestAsync()`. Forward heartbeats to `IDeviceRepository` (update `LastHeartbeatAtUtc` and `Status`). Implement `MqttDeviceEventPublisher : IDeviceEventPublisher` for command dispatch.
  - **Verification:** Integration test starts Mosquitto container, publishes a test event via MQTTnet, asserts that `DeviceEvents` table has a new row with the correct `TenantId`, `DeviceId`, and `EventType`.

- [ ] **6.4** **Device event ingestion pipeline.** Implement `MqttDeviceEventConsumer : IDeviceEventConsumer` that receives raw MQTT payloads, resolves the `IDeviceHandler` for the device type, calls `NormalizeEventAsync` to produce a `DeviceEvent`, and persists via `IDeviceEventRepository`. Publish domain events (`DeviceRegistered`, `DeviceWentOffline`, `BiometricMatchOccurred`) via an in-process mediator.
  - **Verification:** Integration test: send a `motion_detected` event for a camera; assert `DeviceEvents` row exists; send a `biometric_match` event for a biometric reader; assert `DeviceEvents` row exists with `EventType = "biometric_match"` and `PayloadJson` contains the confidence score.

- [ ] **6.5** **Biometric encryption service.** Implement `BiometricEncryptionService : IBiometricService` using `System.Security.Cryptography.AesGcm`. Encrypt templates before storage. Decrypt only for verification sessions (template is sent to the reader device via MQTT command, never stored decrypted). Implement `EnrollAsync`, `VerifyAsync` (sends MQTT command and awaits result), and `DeleteAsync` (permanent delete, no soft-delete, LGPD). Master key from `Biometric__MasterKeyBase64` env var / Docker secret.
  - **Verification:** Unit test encrypts a byte array, asserts ciphertext differs from plaintext, decrypts, asserts roundtrip matches. Integration test: enroll a template, attempt verify (mock MQTT response), delete template, assert row is gone from `BiometricTemplates`.

- [ ] **6.6** **Device health monitoring service.** Implement `DeviceHealthMonitorService : BackgroundService` that periodically (every 30 seconds, configurable) queries `Devices` where `LastHeartbeatAtUtc < now - threshold` and transitions `Status` to `Offline` or `Degraded`. Publish `DeviceWentOffline` domain events.
  - **Verification:** Integration test: insert a device with `LastHeartbeatAtUtc = now - 5 minutes`; run the monitor; assert `Status` changes to `Offline` and `DeviceWentOffline` event is published.

- [ ] **6.7** **HardwareIntegration API endpoints.** Create `Modules/HardwareIntegration/ControlEasyReborn.Modules.HardwareIntegration.Api/` with Minimal API endpoints: `POST /api/v1/devices`, `GET /api/v1/devices?deviceType=&status=`, `GET /api/v1/devices/{id}`, `PUT /api/v1/devices/{id}`, `DELETE /api/v1/devices/{id}`, `GET /api/v1/devices/{id}/events?from=&to=&eventType=`, `GET /api/v1/devices/{id}/health`, `POST /api/v1/devices/{id}/heartbeat`, `POST /api/v1/devices/{id}/biometric-templates`, `GET /api/v1/devices/{id}/biometric-templates`, `DELETE /api/v1/biometric-templates/{id}`, `GET /api/v1/devices/{id}/stream-url`. All device CRUD endpoints enforce `RequirePermission("devices.manage")`. Read endpoints use `RequirePermission("devices.read")`. Biometric endpoints use `RequirePermission("biometrics.enroll|read|delete")`. Register in `Host/Program.cs` via `AddHardwareIntegrationModule()`.
  - **Verification:** Integration test: register a device, list devices, update config, get health, delete device. Cross-tenant test: tenant A device is invisible to tenant B (404 on GET, 403 on PUT).

- [ ] **6.8** **HardwareIntegration database schema.** The `Devices`, `DeviceEvents`, and `BiometricTemplates` tables are in `05-photos-and-devices-schema.sql` (created in Phase 5). Add them to the tenant backfill script. Add NetArchTest rule ensuring these tables are listed.
  - **Verification:** `docker compose down -v && docker compose up -d` — MySQL init succeeds; `Devices`, `DeviceEvents`, `BiometricTemplates` tables exist with `tenant_id` column.

- [ ] **6.9** **Docker Compose additions.** Add `mosquitto`, `mediamtx`, and `minio` services to `docker/docker-compose.yml`. Add `docker/mosquitto/mosquitto.conf` and `docker/mosquitto/passwd` (with default dev credentials). Add `docker/mediamtx/mediamtx.yml` (HLS + WebRTC enabled). Add `minio` init script to create the `controleasy-photos` bucket on first boot.
  - **Verification:** `docker compose up -d` — all new services start healthy; `mosquitto` accepts connections on port 1883; `minio` console is accessible on port 9001; `mediamtx` HLS endpoint is accessible on port 8888.

- [ ] **6.10** **Angular HardwareIntegration module.** Create `HardwareIntegrationModule` (lazy-loaded at `/devices`) with:
  - `DeviceListComponent` (table of devices, filter by type/status, links to detail)
  - `DeviceDetailComponent` (device config, health status indicator, event timeline)
  - `DeviceHealthIndicatorComponent` (online=green, offline=red, degraded=yellow, maintenance=gray)
  - `DeviceEventFeedComponent` (real-time event list using SignalR `DeviceEventHubService`)
  - `CameraStreamViewerComponent` (HLS video player using `<video>` tag + HLS.js for surveillance, WebRTC for intercom)
  - `BiometricEnrollmentDialogComponent` (admin-only, LGPD consent form, calls `POST /api/v1/devices/{id}/biometric-templates`)
  - `DeviceService` (Angular service for `/api/v1/devices/*`)
  - `DeviceEventHubService` (SignalR connection to `/hubs/devices`, tenant-scoped events)
  - `StreamUrlService` (gets presigned HLS/WebRTC URLs from `GET /api/v1/devices/{id}/stream-url`)
  - Permission guard: `CanActivate` checking `devices.read|manage`, `biometrics.enroll|read|delete`
  - **Verification:** Open `https://localhost/devices` in Chrome via Playwright; see device list; click a device; see health status; if a camera is configured, see the HLS stream.

- [ ] **6.11** **SignalR hub for real-time device events.** Add `DeviceEventHub : Hub` in `Modules/HardwareIntegration.Api/` that broadcasts device events to connected clients filtered by `tenant_id`. The `MqttDeviceEventConsumer` publishes events to this hub after persisting. Angular `DeviceEventHubService` subscribes to the hub and updates the UI in real-time.
  - **Verification:** Integration test: start API + Mosquitto; publish a `motion_detected` event via MQTT; assert SignalR client receives the event within 3 seconds.

- [ ] **6.12** **Camera stream URL generation.** Implement `CameraDeviceHandler.GetStreamUrl()` that returns a presigned HLS or WebRTC URL based on the device's `ConfigJson` (which contains the RTSP source URL). The URL points to MediaMTX's HLS/WebRTC endpoint for the device's stream.
  - **Verification:** Unit test: given a `Device` with `DeviceType = Camera` and an RTSP URL in `ConfigJson`, `GetStreamUrl` returns a valid HLS URL pointing to `http://mediamtx:8888/stream/{deviceId}/index.m3u8`.

- [ ] **6.13** **MQTT + Device integration test.** Add Testcontainers-based integration test that boots Mosquitto + MySQL + API, registers a device via the API, publishes a heartbeat and an event via MQTT, and verifies: (a) device `Status` changes to `Online`, (b) `DeviceEvents` table has a row, (c) SignalR client receives the event.
  - **Verification:** `dotnet test` — all device integration tests pass.

- **Verification gate (Phase 6):**
  - `docker compose up -d` — all services healthy (including `mosquitto`, `mediamtx`, `minio`).
  - Register a camera device via `POST /api/v1/devices` → 201 Created.
  - Publish a heartbeat via MQTT → device `Status` changes to `Online`.
  - Publish a `motion_detected` event via MQTT → `GET /api/v1/devices/{id}/events` returns the event.
  - `GET /api/v1/devices/{id}/stream-url` returns a valid HLS URL.
  - Enroll a biometric template → stored encrypted (assert `TemplateEncrypted` is not plaintext).
  - Delete the biometric template → row is gone (hard-delete, LGPD compliance).
  - Angular device dashboard renders in Chrome; real-time events appear within 3 seconds of MQTT publish.
  - Cross-tenant test: tenant A device is invisible to tenant B (404 on GET, 403 on PUT).
  - `dotnet test` — all unit + integration + architecture tests pass.

---

## Phase 7 — Cross-Module Integration & Hardening

*Goal: wire the Photos and HardwareIntegration modules into existing modules (Residents, Visits, Vehicles, Security), add end-to-end tests, harden security, and prepare for production.*

- [ ] **7.1** **Photo attachments on existing entities.** Add navigation properties / query extensions in Residents, Visits, Vehicles, and ServiceProviders modules so that `GET /api/v1/residents/{id}` includes a `photoUrl` field, `GET /api/v1/visits/{id}` includes `photoIds`, etc. The Photos module is the owner of the photo data; other modules reference it by `EntityType` + `EntityId`.
  - **Verification:** Integration test: create a resident, upload a photo for that resident, `GET /api/v1/residents/{id}` includes the `photoUrl`.

- [ ] **7.2** **Biometric match triggers visit check-in.** When a `BiometricMatchOccurred` domain event is published (from `MqttDeviceEventConsumer`), the Visits module subscribes and creates a visit check-in automatically. The event carries `DeviceId` (which gatehouse), `ResidentId` (who matched), and `ConfidenceScore`. The Visits module creates a `Visit` with `CheckInAtUtc = event.OccurredAtUtc` and `GatehouseId = device.GatehouseId`.
  - **Verification:** Integration test: publish a `biometric_match` MQTT event with a valid resident ID; assert a new `Visit` is created with the correct resident, gatehouse, and timestamp.

- [ ] **7.3** **Device-gatehouse linking in Security module.** Add `Devices` link to the `Gatehouse` entity (via `Device.GatehouseId` FK). Update `GET /api/v1/security/gatehouses` to include device counts per gatehouse. Update `GET /api/v1/devices` to accept `gatehouseId` filter.
  - **Verification:** Integration test: register a device with a gatehouse; `GET /api/v1/security/gatehouses` includes `deviceCount`; `GET /api/v1/devices?gatehouseId={id}` returns the device.

- [ ] **7.4** **Audit logging for photo access and biometric operations.** Add Serilog enrichers that log `PhotoId`, `DeviceId`, and `BiometricTemplateId` on every photo upload/download/delete and every biometric enroll/verify/delete. The log entries include `TenantId`, `UserId`, action, and timestamp. These are queryable in Seq.
  - **Verification:** Upload a photo, view it, delete it; check Seq logs for three entries with `PhotoId`, `TenantId`, `UserId`, and action.

- [ ] **7.5** **LGPD/GDPR compliance: photo and biometric deletion flows.** Implement the right-to-erasure endpoint: `DELETE /api/v1/residents/{id}/personal-data` (TenantAdmin only) that cascades to: soft-delete all photos, hard-delete all biometric templates, and log the erasure request with the requesting user's ID and timestamp.
  - **Verification:** Integration test: create a resident with photos and biometric templates; call the erasure endpoint; assert all photos are soft-deleted (`IsActive = false`), all biometric templates are hard-deleted (rows gone), and an audit log entry exists.

- [ ] **7.6** **End-to-end test: gatehouse scenario.** Write a Playwright test that: (a) logs in as an attendant, (b) opens the gatehouse dashboard, (c) registers a camera device, (d) sees the camera stream, (e) captures a visitor photo, (f) sees the photo in the visit detail page, (g) receives a real-time device event (simulated via MQTT publish), (h) verifies the event appears in the dashboard within 3 seconds.
  - **Verification:** Playwright test passes end-to-end in Chrome.

- [ ] **7.7** **Performance baseline.** Benchmark `POST /api/v1/photos/upload` (p95 < 500ms for a 5MB photo), `GET /api/v1/devices/{id}/events` (p95 < 200ms for 1000 events), and `GET /api/v1/photos/{id}/thumbnails/small` (p95 < 100ms). Record baselines in `docs/performance/baselines.md`.
  - **Verification:** Benchmark test passes; baselines documented.

- [ ] **7.8** **Architecture tests.** Add NetArchTest rules: (a) `Photos.Domain` does not reference `Photos.Infrastructure`, (b) `HardwareIntegration.Domain` does not reference `HardwareIntegration.Infrastructure`, (c) no project references `MySql.Data` or `EntityFramework`, (d) `Photos` and `HardwareIntegration` module entities all have `TenantId`, (e) `BiometricEncryptionService` uses `AesGcm` (not `AesManaged`), (f) `IStorageProvider` is the only storage abstraction (no direct `System.IO.File` calls in the Application layer).
  - **Verification:** `dotnet test` — architecture tests pass.

- **Verification gate (Phase 7):**
  - All cross-module integrations work (photo attachments on residents/visits/vehicles, biometric check-in, device-gatehouse linking).
  - Audit logs are present in Seq for photo and biometric operations.
  - LGPD erasure flow works end-to-end.
  - Playwright gatehouse scenario test passes.
  - Performance baselines are documented and met.
  - Architecture tests pass.
  - `git grep -ri "EntityFramework|MySql.Data" src/` returns nothing.

---

## Task Dependency Graph

```json
{
  "waves": [
    {
      "wave": 1,
      "tasks": ["5.1", "5.2", "5.8", "6.1", "6.2", "6.8", "6.9"]
    },
    {
      "wave": 2,
      "tasks": ["5.3", "5.4", "5.5", "5.6", "6.3", "6.4", "6.5", "6.6", "6.12"]
    },
    {
      "wave": 3,
      "tasks": ["5.7", "5.9", "5.10", "6.7", "6.10", "6.11"]
    },
    {
      "wave": 4,
      "tasks": ["6.13"]
    },
    {
      "wave": 5,
      "tasks": ["7.1", "7.2", "7.3", "7.4", "7.5", "7.7", "7.8"]
    },
    {
      "wave": 6,
      "tasks": ["7.6"]
    }
  ]
}
```

**Dependency rationale:**
- Wave 1: Domain entities, Application abstractions, database schema, and Docker services are independent of each other and can be built in parallel.
- Wave 2: Infrastructure implementations (storage providers, thumbnail service, MQTT client, biometric encryption, health monitor) depend on Wave 1 abstractions.
- Wave 3: API endpoints and Angular UI depend on Wave 2 implementations. SignalR hub depends on MQTT consumer.
- Wave 4: Integration tests that exercise the full MQTT → API → DB → SignalR pipeline depend on all prior waves.
- Wave 5: Cross-module integrations and hardening depend on both modules being complete.
- Wave 6: End-to-end Playwright test depends on everything being wired up.

---

## Continuous (every phase)

- [ ] **C.7** After every API change in Photos or HardwareIntegration modules, run `ng-openapi-gen` to regenerate the Angular TypeScript client and fix any breaking call sites.
- [ ] **C.8** Every UI change (photo gallery, device dashboard) is verified in a real Chrome browser via the Playwright tool before being marked done.
- [ ] **C.9** All `Photos` and `HardwareIntegration` entities have `TenantId` (C.6 architecture rule extended). Every integration test for these modules includes a cross-tenant-access assertion.
- [ ] **C.10** Serilog enrichment: every photo and device event log entry includes `TenantId`, `DeviceId`, `PhotoId`, `UserId` as structured properties.