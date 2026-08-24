# Requirements — Door Integration (Optional)

> Spec folder: `.specs/door-integration/`
> Milestone: v2.1 — Door Integration (Optional)
> Phases: 14 (door relay & unlock commands), 15 (reader events & device health)
> Supersedes: `.specs/_retired/3 - photo-capture-hardware-integration/` (hardware framework subset — redesigned as integration, not platform)

## Context

Condominiums that opt in can integrate ControlEasy with their door relay and card/biometric readers. The API triggers the relay with HMAC-signed commands; reader events create entry log records automatically. **The door opens independently of ControlEasy** — the API is a participant, not a gatekeeper. If ControlEasy is offline, the card reader and manual release still work. ControlEasy logs the event when it reconnects.

This is not a "pluggable hardware framework." It's *integrations* — one vendor at a time, starting from a real deployment. The `IDeviceHandler` abstraction emerges from the second integration, not before it.

The cameras belong to the condominium. ControlEasy does not deploy, manage, or proxy cameras. The CCTV is external; ControlEasy carries timestamps that the syndic cross-references with footage.

## User Stories

### Door Relay

- **UC-DI-01:** As a *gatehouse attendant*, I want to click "open gate" in the ControlEasy UI and have the gate physically open so I don't need a separate remote or physical button.
- **UC-DI-02:** As a *platform architect*, I want the door to open independently of ControlEasy (hardware fallback — card reader and manual release work without the API) so a ControlEasy outage doesn't lock residents out of their homes.
- **UC-DI-03:** As a *security officer*, I want every API-triggered unlock command to be HMAC-signed, rate-limited, authorized (porteiro role only), and audit-logged so a compromised API token can't open the gate without attribution.
- **UC-DI-04:** As a *tenant administrator*, I want door integration to be opt-in per condominium so condominiums without hardware don't carry its complexity or risk.
- **UC-DI-05:** As a *security officer*, I want replay attacks on unlock commands to fail (nonce + timestamp window) so a captured command can't be replayed later to open the gate.

### Reader Events

- **UC-DI-06:** As a *dweller (morador)*, I want to tap my access card at the reader and have the gate open + the entry logged automatically so I don't need the porteiro to manually register my entry.
- **UC-DI-07:** As a *gatehouse attendant*, I want unknown cards to be denied at the gate (gate stays closed) and the attempt logged so I can review access attempts by unknown credentials.
- **UC-DI-08:** As a *platform architect*, I want reader events normalized into a unified `DeviceEvent` model (`device_id`, `event_type`, `payload_json`, `occurred_at`) so business logic remains vendor-agnostic.
- **UC-DI-09:** As a *platform architect*, I want the `IDeviceHandler` abstraction to emerge from two real integrations (not built speculatively) so the abstraction is proven, not theoretical.

### Device Health

- **UC-DI-10:** As a *tenant administrator*, I want to see device health (online/offline, last heartbeat, error rate) so I can proactively maintain the hardware at my condominium's entrances.
- **UC-DI-11:** As a *gatehouse attendant*, I want an alert toast when a device goes offline so I know the reader is down and I need to use manual entry.
- **UC-DI-12:** As a *platform architect*, I want event replay when ControlEasy reconnects after downtime (queued events on reader or local gateway) so entries that happened during the outage are not lost.

## Requirements

### DOOR-01: Door Relay & Unlock Commands

- `IDoorController` abstraction — `UnlockAsync(deviceId, command)` with vendor-specific implementations
- First implementation: one real condominium's door controller (vendor TBD)
- `POST /api/v1/devices/{deviceId}/unlock` — porteiro-role only, HMAC-SHA256 signed command, rate-limited (max 1 unlock per 3 seconds per device), audit-logged
- Command audit: requesting user, device ID, timestamp, command hash, response status — append-only, tamper-evident
- `devices` table: `id`, `tenant_id`, `device_type`, `vendor`, `connection_params` (encrypted), `gatehouse_id`, `is_active`
- Tenant opt-in: `door_integration_enabled` flag — default false; when false, no door endpoints exposed
- Fallback: door controller operates independently (card reader + manual release work without API)
- Event queue with replay: ControlEasy offline → reader queues events → reconnect → replay
- Security: HMAC-SHA256 per-tenant key, nonce + timestamp window (replay protection), porteiro role required, rate limit
- Threat model document: remote unlock attack surface, key management, replay prevention

### DOOR-02: Reader Events & Entry Log Integration

- `IDeviceHandler` abstraction — `HandleEventAsync(deviceEvent)` — strategy per device type. Built from two real integrations, not speculative.
- First reader integration: one real condominium's card reader or biometric scanner
- Event ingestion: reader sends event → `IDeviceHandler` normalizes to `DeviceEvent` → entry log created
- Card matches registered dweller → `entered_with_consent` (dweller pre-consented at registration)
- Unknown card → `denied` (or porteiro override per policy)
- `device_events` table: `id`, `tenant_id`, `device_id`, `event_type`, `payload_json`, `occurred_at`, `processed_at`, `entry_log_id` (nullable)
- Event replay: `processed_at` tracks ingestion time vs event time; queued events replayed on reconnect

### DOOR-03: Device Health Monitoring

- `device_heartbeats` table: `device_id`, `last_heartbeat_at`, `status` (online/offline/degraded), `error_count`
- Heartbeat ingestion: reader sends periodic heartbeat → updates `last_heartbeat_at`
- Alert thresholds per device: `offline_after_seconds` (default 60), `error_rate_threshold` (default 10%)
- Health dashboard UI: per-device status, last heartbeat, error rate, alert badges
- Alert toast in porteiro UI when device goes offline

## Architecture

```
Card/biometric reader (autonomous) ─┐
                                    ├──→ Door relay (hardware)
Porteiro UI → ControlEasy API ──────┘            │
                                    │            ▼
                                    └──→ Event to ControlEasy (async)
```

The door opens via *either* path. ControlEasy logs events from *either* path. ControlEasy offline ≠ locked out.

## Design Principles

1. **ControlEasy is a participant, not a gatekeeper** — the door opens independently; the API observes and can trigger, but doesn't block physical access.
2. **No speculative abstractions** — `IDeviceHandler` emerges from the second real integration. First integration is one vendor, one driver.
3. **Hardware integration is opt-in** — condominiums without hardware use the voluntary ledger (v2.0). With hardware, the ledger becomes enforced (door event creates the log entry).
4. **Security-sensitive feature** — remote unlock is a new attack surface. Own threat model, own adversarial review, own milestone.
5. **Cameras are external** — ControlEasy does not deploy, manage, or proxy cameras. The CCTV belongs to the condominium. ControlEasy carries timestamps.

## Out of Scope

- Camera integration / NVR proxy / WebRTC / media server — cameras belong to the condominium
- MQTT-based sensor ingestion framework — not building a platform; building integrations
- Biometric template storage and encryption (AES-256-GCM) — no biometrics in v2.1; card readers only
- Video recording and storage — future spec
- AI-powered facial recognition — future spec
- Multi-vendor support from day one — one vendor, one condominium, then the second proves the abstraction
- Approval workflow for overrides — v2.0 ships visibility, not enforcement

## Traceability

| Requirement | Phase | Spec |
|---|---|---|
| DOOR-01 | Phase 14 | `.specs/door-integration/` |
| DOOR-02 | Phase 15 | `.specs/door-integration/` |
| DOOR-03 | Phase 15 | `.specs/door-integration/` |

---
*Requirements defined: 2026-08-23*
*Derived from: party-mode v2.0/v2.1 milestone design session*