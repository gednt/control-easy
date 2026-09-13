# Phase 12: Photo Capture & Display - Context

**Gathered:** 2026-09-12
**Status:** Ready for planning
**Mode:** Smart discuss (autonomous)

<domain>
## Phase Boundary

Browser-based photo capture + upload UI for ControlEasy Reborn's gatehouse workflow. The porteiro opens a resident/visitor/vehicle/service-provider record, clicks "Take photo" (camera) or "Upload photo" (file picker), and the photo is captured/compressed client-side and uploaded to the existing Phase 11 Photos API. Photos display as thumbnails in detail pages and table rows, with a lightbox view of the source.

Phase 11 (Photos & Consent Schema) shipped the backend (POST/GET/DELETE /api/v1/photos, storage abstraction, append-only audit log). Phase 12 delivers only the client-side capture/upload/display UI plus integration into entity pages.

Phase 13 (Consent gatehouse workflow UI) is a separate phase; it will reuse `ce-photo-capture` and `ce-photo` components from Phase 12.

</domain>

<decisions>
## Implementation Decisions

### Capture Flow (modal vs inline vs fullscreen)

- Modal with live camera preview at top, bottom-bar capture button (shutter icon), cancel/retake controls, on-capture → compression → upload
- Modal is invoked from a "Take photo" button on the entity detail page header

### File Upload Mode (separate from camera)

- Two buttons on entity detail page header: "Take photo" → camera modal, "Upload photo" → file picker modal
- Camera modal and upload modal share the same post-capture flow (compression → upload → thumbnail generation → submit)

### Compression Strategy (quality ladder)

- Resize longest dimension to ≤1280px
- Encode as JPEG; initial quality 0.8
- If output > 500KB → retry at quality 0.6
- If still > 500KB → retry at quality 0.3 (floor)
- If still > 500KB after floor → reject with "file too large after compression" error

### Upload Retry Behavior

- 3 attempts with exponential backoff: 1s, 2s, 4s
- Show retry counter and spinner on the upload button during retry
- On final failure → toast with "Try again" button

### EXIF Stripping & Verification

- Canvas redraw strips EXIF automatically (canvas → blob has no EXIF header)
- Playwright integration test verifies uploaded blob has no GPS/camera-model/timestamp EXIF data using exifr library
- Defensive: also strip the source file's EXIF before canvas redraw (redundant but documented)

### Photo Display (table + lightbox)

- 128×128 thumbnail in `ce-table` rows (lazy-loaded)
- Click thumbnail → lightbox modal with source at 800px max-width
- Lightbox has prev/next controls for multi-photo galleries
- Multiple photos per entity supported (gallery pattern; most recent as primary thumbnail)

### Entity Page Integration

- All four entity types in this phase: residents, visitors, vehicles, service-providers
- Same `ce-photo-capture` component, same props, integrated into detail page header (next to the existing edit/deactivate buttons)
- Photos fetched on entity load; new uploads append to the gallery
- Soft-delete from gallery (uses existing `DELETE /api/v1/photos/{id}` from Phase 11)

### Error Handling (toast + fallback)

- Camera permission denied → toast + automatic fallback to file picker
- Upload failed after 3 retries → toast with "Try again" button (does not block modal)
- File too large after compression floor → toast with "Try a smaller photo" message
- File type rejected (not JPEG/PNG/HEIC) → toast with accepted types list
- All errors preserve captured photo in modal until user dismisses

### Component Architecture

- `ce-photo-capture` — input: `{ entityType, entityId, onComplete }`; output: new photo event
- `ce-photo` — display component: `{ photoId, size: 'thumbnail' | 'source', clickable }`; lazy-loads via `GET /api/v1/photos/{id}/thumbnail` or `/source`
- `ce-photo-gallery` — wraps multiple `ce-photo` thumbnails with lightbox state
- `ce-photo-lightbox` — full-screen modal showing source + prev/next controls
- All components: standalone, OnPush, signals for state

### the agent's Discretion

- Exact modal dimensions, animation timing, button positions (matches mockup visual parity from Phase 9 where applicable)
- Lightbox transition effects (fade vs slide)
- Thumbnail placeholder while loading (skeleton vs blurhash)
- Gallery layout (grid vs horizontal scroll)
- EXIF metadata fields to verify in test (GPS only, or GPS + camera model + timestamp)

</decisions>

<code_context>
## Existing Code Insights

### Reusable Assets

- `ce-modal` component (`src/Web/.../design-system/components/modal`) — used by residents page edit modal; can host the photo capture modal
- `ce-button`, `ce-icon`, `ce-spinner`, `ce-toast` — design-system primitives already used everywhere
- `ce-table` — used by residents page; supports custom cell renderers (can plug thumbnail column)
- `ce-card` — used for entity detail page headers; will host photo gallery
- OpenAPI-generated TypeScript client lives at `src/Web/.../src/app/api/` (per AGENTS.md, generated but not yet consumed by handwritten services — handwritten `PhotosApiService` is the v2 pattern)
- `PhotosApiService` already exists in Phase 11 backend integration (need to verify handwritten vs generated)

### Established Patterns

- Standalone components with OnPush change detection (per AGENTS.md)
- Signals API for reactive state (`signal`, `computed`, `effect`)
- Reactive forms for any user inputs (not needed for camera capture but used in modals)
- HTTP services return Observables; use async pipe or convert to signal via `toSignal`
- OpenAPI client at `src/app/api/` (per `npm run openapi-check` workflow)
- Service naming: `{Entity}ApiService` (e.g., `ResidentsApiService`)
- Component naming: kebab-case selectors with `ce-` prefix

### Integration Points

- `src/Web/.../src/app/features/residents/residents.page.ts` — add photo gallery section to detail modal
- `src/Web/.../src/app/features/visits/` (visitor subdir TBD) — detail page needs photo gallery
- `src/Web/.../src/app/features/vehicles/vehicles.page.ts` — add photo gallery
- `src/Web/.../src/app/features/service-providers/` — detail page needs photo gallery
- `src/Web/.../src/app/design-system/index.ts` — export new `ce-photo-capture`, `ce-photo`, `ce-photo-gallery`, `ce-photo-lightbox` components
- `src/Web/.../src/app/api/` — handwritten `PhotosApiService` should use the existing `POST /api/v1/photos`, `GET /api/v1/photos/{id}`, `GET /api/v1/photos/{id}/thumbnail`, `DELETE /api/v1/photos/{id}` endpoints from Phase 11

</code_context>

<specifics>
## Specific Ideas

- Capture flow must complete under 5 seconds on gatehouse WiFi (ROADMAP success criterion #1) — compression and upload both client-side to minimize round-trips
- 8MB iPhone HEIC → ≤500KB JPEG (ROADMAP success criterion #2) — implies HEIC decoding via canvas (Safari-only) or browser-native HEIC support
- EXIF strip verified by automated test (ROADMAP success criterion #3) — use exifr library
- 3 retries with backoff (ROADMAP success criterion #4) — implementation matches exactly
- Lightbox supports multi-photo galleries (ROADMAP success criterion #5) — prev/next navigation required
- Photos on all four entity pages (ROADMAP success criterion #6) — see decisions above
- Playwright E2E with mocked `getUserMedia` (ROADMAP success criterion #7) — mock navigator.mediaDevices.getUserMedia in test setup
- `dotnet test` + `npm test` green; Docker stack healthy (ROADMAP success criterion #8) — integration tests + Angular unit tests + Playwright E2E

No additional specific requirements beyond ROADMAP. The decisions above capture the design intent.

</specifics>

<deferred>
## Deferred Ideas

- Bulk photo upload (multiple files at once) — out of scope; single-photo capture flow only
- Photo cropping / rotation before upload — out of scope; capture-as-is, accept orientation metadata stripped by canvas
- Photo annotation (draw, text) — out of scope
- Live face detection / autofocus — out of scope; rely on browser native camera controls
- HEIC encoding for output (keep all output as JPEG) — Phase 12 only ships JPEG output; HEIC input is accepted and converted to JPEG
- Mobile app native camera (PWA / Capacitor) — out of scope; responsive web only
- Photo organization (albums, tags) — out of scope; flat gallery per entity

</deferred>
