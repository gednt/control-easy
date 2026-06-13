# Orchestration Log — Photo Capture & Hardware Integration

> Spec folder: `.specs/3 - photo-capture-hardware-integration/`
> This log tracks the multi-agent work for designing the **photo capture/storage** and **hardware integration framework** (biometric sensors, security cameras) for the ControlEasy Reborn platform.

## Goal

Design a technical implementation plan for two cross-cutting capabilities:

1. **Photo capture and storage** — capture, store, and retrieve photographs of residents, visitors, vehicles, and service providers.
2. **Hardware integration framework** — establish a pluggable architecture for integrating biometric sensors, security cameras, and other IoT devices at condominium entrance points (gatehouses / portarias).

## Execution Plan

| Phase | Agent | Status | Output |
|---|---|---|---|
| 1 | Orchestrator (as Backend) | done | `requirements.md` (UC-26..UC-40), `design.md` (backend sections: Photos module, HardwareIntegration module, database schema, MQTT, biometric encryption, API endpoints, security model, cross-cutting concerns, camera streaming, Docker Compose additions, DI, config, success criteria, risks) |
| 2 | Orchestrator (as Frontend) | done | `frontend-design.md` (Angular module structure, routing, components, services, SignalR, permission guards, visual design notes, responsive layout) |
| 3 | Orchestrator (as DevOps) | done | `infrastructure-design.md` (Docker Compose prod config, MinIO bucket init, Mosquitto TLS+ACL, MediaMTX config, network topology, environment variables, secrets management, biometric key rotation, backup/recovery, monitoring, security hardening, CI/CD, scaling) |
| 4 | Orchestrator | done | `tasks.md` (Phases 5-7 with dependency graph, verification gates) |

## Dependencies on Existing Specs

- Builds on the modular monolith architecture from `.specs/1 - modernization-roadmap/` (Clean Architecture, DBTools_SQL, TenantAwareLinqFactory, JWT auth, multi-tenancy).
- Must follow the visual design system from `.specs/2 - visual-design-system/` for any UI components.
- All new modules inherit the `TenantId` + `ITenantContext` pattern (C.6 architecture rule).
- All new API endpoints follow the existing REST conventions (`/api/v1/...`, kebab-case, plural nouns, ProblemDetails errors).

## Key Decisions (resolved)

1. **Storage backend:** `IStorageProvider` abstraction with `LocalFileStorageProvider` (dev) and `MinioStorageProvider` (prod). Swappable via `appsettings.json` `Storage:Provider` key. Decision: MinIO for its S3 compatibility and self-hosting capability.
2. **Photo metadata schema:** EXIF metadata stored as JSON column (`MetadataJson`) on the `Photos` table. Thumbnails generated server-side (ImageSharp) in three sizes (128x128, 512x512, 1024x1024) and stored alongside originals in MinIO/local filesystem. Capture context (`GatehouseId`, `Source`) stored as columns.
3. **Hardware communication protocol:** MQTT (MQTTnet) with QoS 1 for events, QoS 0 for heartbeats. Topics: `controleasy/{tenant_id}/devices/{device_id}/events/{event_type}`, `commands/{cmd_type}`, `heartbeat`, `status`.
4. **Biometric data handling:** AES-256-GCM encryption at rest. Master key from environment/Docker secret. Decryption only for verification sessions (template sent to reader device via MQTT command, never stored decrypted). LGPD/GDPR: hard-delete only, no soft-delete for biometric templates.
5. **Camera streaming architecture:** MediaMTX as RTSP-to-HLS/WebRTC proxy. Cameras stream RTSP to MediaMTX. Angular SPA gets presigned HLS/WebRTC URLs from the API. MediaMTX is on a separate `iot` Docker network.
6. **Device registry model:** Per-tenant `Devices` table with `DeviceType` enum, `Status` (Online/Offline/Degraded/Maintenance), `LastHeartbeatAtUtc`, and `ConfigJson` for vendor-specific settings. Health monitored by `DeviceHealthMonitorService` background service.

## Coordination Log

### Phase 1 — Orchestrator (Backend design)

- **Completed:** `requirements.md` with 15 user stories (UC-26..UC-40) covering photo capture/storage (7 stories) and hardware integration (8 stories).
- **Completed:** `design.md` with full backend architecture for `Modules/Photos/` and `Modules/HardwareIntegration/`, including domain entities, application abstractions, infrastructure implementations (IStorageProvider, MQTT client, biometric encryption, health monitoring), API endpoints, database schema, security model, cross-cutting concerns, and camera streaming architecture.
- **Key design decisions:** Polymorphic photo-entity association (EntityType + EntityId), strategy pattern for device types (IDeviceHandler), SignalR for real-time device events to Angular, MediaMTX for camera streaming, MinIO for photo storage.

### Phase 2 — Orchestrator (Frontend design)

- **Completed:** `frontend-design.md` with full Angular module structure for Photos and HardwareIntegration modules.
- **Components:** PhotoUploadComponent (drag-and-drop + file picker), CameraCaptureComponent (webcam capture), PhotoGalleryComponent, PhotoViewerComponent, PhotoDeleteDialogComponent, ProfilePhotoComponent, DeviceListComponent, DeviceDetailComponent, DeviceHealthIndicatorComponent, DeviceEventFeedComponent (SignalR), CameraStreamViewerComponent (HLS.js + WebRTC), BiometricEnrollmentDialogComponent, GatehouseDashboardComponent.
- **Services:** PhotoService, DeviceService, DeviceEventHubService (SignalR), StreamUrlService.
- **Key pattern:** Gatehouse dashboard optimized for 1024x768 tablet (60/40 split: camera + events).

### Phase 3 — Orchestrator (Infrastructure design)

- **Completed:** `infrastructure-design.md` with Docker Compose production config (MinIO init, Mosquitto TLS+ACL, MediaMTX), network topology (dmz/backend/iot), environment variables, secrets management (Docker secrets + vault), biometric key rotation procedure, backup/recovery (MinIO versioning, MySQL per-tenant dump, Mosquitto data), monitoring (MinIO health, Mosquitto metrics, device health dashboard), security hardening (MinIO TLS+bucket policies, Mosquitto TLS+ACL, MediaMTX CORS, network segmentation), CI/CD additions (Testcontainers for MinIO+Mosquitto), scaling considerations (MinIO erasure coding, Mosquitto clustering, MediaMTX per-gatehouse, DeviceEvents partitioning, SignalR Redis backplane).

### Phase 4 — Orchestrator (Tasks)

- **Completed:** `tasks.md` with three phases (5, 6, 7) and a dependency graph with 6 waves.
- Phase 5: Photos Module (10 tasks, from domain layer to Angular gallery + integration tests).
- Phase 6: HardwareIntegration Module (13 tasks, from domain layer to MQTT client, biometric encryption, device health, API endpoints, Angular dashboard, SignalR hub, camera streaming).
- Phase 7: Cross-Module Integration & Hardening (8 tasks, from photo attachments on existing entities to LGPD compliance, E2E tests, performance baselines, architecture tests).
- Continuous tasks C.7-C.10 (OpenAPI regeneration, UI verification, TenantId architecture rule, Serilog enrichment).