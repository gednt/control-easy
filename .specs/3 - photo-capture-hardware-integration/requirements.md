# Requirements — Photo Capture & Hardware Integration

> Spec folder: `.specs/3 - photo-capture-hardware-integration/`
> Goal: Extend ControlEasy Reborn with (1) photograph capture, storage, and retrieval for residents, visitors, vehicles, and service providers, and (2) a pluggable hardware integration framework for biometric sensors, security cameras, intercoms, and other IoT devices at condominium entrance points.

## User Stories

### Photo Capture & Storage

- **UC-26:** As a *gatehouse attendant (porteiro)*, I want to capture a photograph of a visitor, resident, or service provider at check-in so the photo is stored and retrievable for future identity verification at the gatehouse.
- **UC-27:** As a *gatehouse attendant*, I want to capture a photograph of a vehicle (license plate, front view, rear view) so it can be compared against registered vehicles during entry and exit.
- **UC-28:** As a *tenant administrator*, I want to view and manage all photos associated with residents, visitors, vehicles, and service providers in my condominium, including soft-deleting photos that are no longer needed.
- **UC-29:** As a *platform architect*, I want all photos stored behind an abstraction layer (`IStorageProvider`) with swappable backends (local filesystem for dev, MinIO/S3 for production) so the storage implementation can change without modifying business logic.
- **UC-30:** As a *security officer*, I want photo access logged in the audit trail and restricted by permission (`photos.read`, `photos.write`, `photos.delete`) so only authorized users can view or modify sensitive images.
- **UC-31:** As a *developer*, I want automatic thumbnail generation (multiple sizes: small 128x128, medium 512x512, large 1024x1024) and EXIF metadata extraction on upload so the frontend can display optimized images without client-side resizing.
- **UC-32:** As a *resident (morador)*, I want to upload or update my own profile photo from the web UI so my identity is visible to gatehouse attendants during verification.

### Hardware Integration Framework

- **UC-33:** As a *tenant administrator*, I want to register and manage hardware devices (cameras, biometric readers, intercoms, barrier gates) at my condominium's entrance points (gatehouses), including assigning devices to specific gatehouses and configuring their connection parameters.
- **UC-34:** As a *platform architect*, I want a pluggable device integration layer (strategy pattern per `DeviceType`) so new hardware types can be added without modifying core business logic — each device type ships its own `IDeviceHandler` implementation.
- **UC-35:** As a *gatehouse attendant*, I want real-time alerts from connected devices (camera motion detected, biometric match/no-match, intercom call) displayed in the web UI via WebSocket/SSE so I can respond to gatehouse events immediately.
- **UC-36:** As a *platform architect*, I want device events ingested via MQTT (QoS 1 for events, QoS 0 for heartbeats) so the system can handle high-throughput, low-latency sensor data from multiple gatehouses simultaneously without blocking the API.
- **UC-37:** As a *security officer*, I want biometric templates stored encrypted at rest (AES-256-GCM) and never transmitted in plaintext so the system complies with LGPD (Lei Geral de Protecao de Dados) and GDPR requirements for biometric data processing.
- **UC-38:** As a *developer*, I want a unified `DeviceEvent` model that normalizes events from different hardware vendors into a common schema (`device_id`, `event_type`, `payload_json`, `occurred_at`) so business logic remains vendor-agnostic.
- **UC-39:** As a *tenant administrator*, I want device health monitoring (online/offline status, last heartbeat timestamp, error rates) with configurable alert thresholds so I can proactively maintain the hardware at my condominium's entrances.
- **UC-40:** As a *platform architect*, I want camera streams proxied through a media server (HLS for standard feeds, WebRTC for low-latency intercom) so the web UI can display live feeds without exposing camera credentials or internal network IPs.

---

## Out of Scope (for this spec)

- AI-powered facial recognition or automated matching (future spec — the framework enables it, but the feature itself is deferred).
- Automatic license plate recognition (ALPR) integration (future spec).
- Real-time video analytics (motion detection on server side, object detection — future spec).
- Push notifications to mobile devices (separate spec).
- Native mobile apps (PWA/responsive web is sufficient for v1).
- Integration with specific hardware vendors beyond the generic MQTT event model (vendor-specific `IDeviceHandler` implementations are future specs).
- Video recording and storage (live streaming only in v1; recording/archival is a future spec).