# Retired Spec: 3 - photo-capture-hardware-integration

**Retired:** 2026-08-23
**Reason:** Superseded by v2.0 milestone design. The original spec bundled photo capture, storage, a pluggable hardware framework (MQTT, WebRTC, biometric encryption, media server, device health monitoring) into a single 15-story phase. The v2.0 design splits this into:
- **v2.0** (Phases 11–13): Photo capture + consent policy + gatehouse entry log — no hardware
- **v2.1** (Phases 14–15): Door relay + reader events — optional, gated on real condominium hardware

The original spec's hardware framework (`IDeviceHandler`, MQTT ingestion, WebRTC camera proxy, biometric AES-256-GCM encryption) was scoped for a product ControlEasy is not yet building. Cameras belong to the condominium (external CCTV), not ControlEasy. The `IDeviceHandler` abstraction will emerge from the second real integration, not be built speculatively.

**Superseded by:**
- `.specs/photo-capture/` (v2.0 Phase 11–12)
- `.specs/consent-gatehouse/` (v2.0 Phase 13)
- `.specs/door-integration/` (v2.1 Phase 14–15)

**Original contents preserved** in this folder for reference.