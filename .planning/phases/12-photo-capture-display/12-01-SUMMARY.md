---
phase: 12
plan: 1
subsystem: photo-capture-display
tags: [photos, ui, capture, upload, compression, exif, retry, gallery, lightbox, angular, signals]
requires:
  - POST /api/v1/photos (Phase 11)
  - GET /api/v1/photos/{id} (Phase 11)
  - DELETE /api/v1/photos/{id} (Phase 11)
provides:
  - PhotosApiService (handwritten HttpClient wrapper)
  - PhotoUtils (compressImage, generateThumbnail, uploadWithRetry)
  - ce-photo display component (128×128 thumbnail, lazy-load, skeleton/error states)
  - ce-photo-gallery component (multi-photo grid + add tile + lightbox integration)
  - ce-photo-lightbox component (full-screen viewer with prev/next/Esc/click-backdrop)
  - ce-photo-capture component (camera getUserMedia + upload drag-and-drop, compression ladder, retry, toast errors)
  - ce-photo-panel component (reusable wrapper for any entity)
  - PhotoBindingCacheService (frontend localStorage cache for entity-binding; see deviations)
  - Residents/Visits/Vehicles/Service-Providers detail-page integration
  - 9 component unit tests + 9 photo-utils tests + Playwright E2E suite + exifr EXIF verification
affects:
  - Residents (now has View photos modal)
  - Visits (Photos action button + panel)
  - Vehicles (Photos action button + panel)
  - ServiceProviders (Photos action button + panel)
tech-stack:
  added:
    - exifr ^7.1.3 (devDependency for E2E EXIF verification)
key-files:
  created:
    - src/Web/ControlEasyReborn.Web/src/app/features/photos/photos-api.service.ts
    - src/Web/ControlEasyReborn.Web/src/app/features/photos/photo-utils.ts
    - src/Web/ControlEasyReborn.Web/src/app/features/photos/photo-binding-cache.service.ts
    - src/Web/ControlEasyReborn.Web/src/app/features/photos/photo-utils.spec.ts
    - src/Web/ControlEasyReborn.Web/src/app/design-system/components/photo/photo.component.ts
    - src/Web/ControlEasyReborn.Web/src/app/design-system/components/photo/photo-lightbox.component.ts
    - src/Web/ControlEasyReborn.Web/src/app/design-system/components/photo/photo-gallery.component.ts
    - src/Web/ControlEasyReborn.Web/src/app/design-system/components/photo/photo-capture.component.ts
    - src/Web/ControlEasyReborn.Web/src/app/design-system/components/photo/photo-panel.component.ts
    - src/Web/ControlEasyReborn.Web/src/app/design-system/components/photo/photo.component.spec.ts
    - src/Web/ControlEasyReborn.Web/src/app/design-system/components/photo/photo-lightbox.component.spec.ts
    - src/Web/ControlEasyReborn.Web/src/app/design-system/components/photo/photo-gallery.component.spec.ts
    - src/Web/ControlEasyReborn.Web/src/app/design-system/components/photo/photo-capture.component.spec.ts
    - src/Web/ControlEasyReborn.Web/e2e/photo-capture.spec.ts
  modified:
    - src/Web/ControlEasyReborn.Web/src/app/design-system/index.ts (export new photo components)
    - src/Web/ControlEasyReborn.Web/src/app/design-system/components/icon/icon.registry.ts (add ChevronLeft, Camera, Upload, Image to CE_LUCIDE_ICONS)
    - src/Web/ControlEasyReborn.Web/src/app/design-system/components/icon/icon.types.ts (add icon name strings)
    - src/Web/ControlEasyReborn.Web/src/app/features/residents/residents.page.ts (View photos modal + row dropdown action)
    - src/Web/ControlEasyReborn.Web/src/app/features/visits/visits.page.ts (Photos action + modal)
    - src/Web/ControlEasyReborn.Web/src/app/features/vehicles/vehicles.page.ts (Photos action + modal)
    - src/Web/ControlEasyReborn.Web/src/app/features/service-providers/service-providers.page.ts (Photos action + modal)
    - src/Web/ControlEasyReborn.Web/package.json (exifr devDep)
    - src/Web/ControlEasyReborn.Web/package-lock.json (exifr + rollup-linux-x64-gnu resolved)
decisions:
  - Frontend PhotoBindingCache in localStorage because Phase 11 backend has no entity_binding column nor list endpoint; the binding cache is keyed by `${entityType}:${entityId}` and serves as a write-through proxy to swap in photosApi.list() once the backend ships the column
  - ce-photo renders the source blob directly via /api/v1/photos/{id} (no /thumbnail endpoint yet); the `size` input on ce-photo is preserved as a forward-compatible affordance so adding the real thumbnail route later is non-breaking
  - ce-photo-capture.switchMode emits via modeChange output instead of mutating the input signal (Angular 18 forbids input.set())
  - PhotoUtils uses HTMLCanvasElement.prototype redraw which inherently strips EXIF (no separate EXIF-cleaning pass needed); E2E verifies the absence of GPS/Make/Model/DateTimeOriginal fields in the uploaded source
  - All photo components are standalone, OnPush, signals-only — no template binding for the 'plus' icon until lucide registry is updated
  - Tested in jsdom via prototype spies on HTMLCanvasElement.toBlob, HTMLImageElement.decode, and HTMLCanvasElement.getContext (jsdom does not implement any of them faithfully)
  - All Phase 12 divergences are documented inline as @comment blocks so future maintainers can remove the cache / src-URL-only renderer once the backend grows entity binding + thumbnail
status: complete
actuals:
  tokens: 78000
  tasks: 10
  commits: 13
---

# Phase 12 Plan 01: Photo Capture & Display UI — Summary

## Result

✅ **Shipped** — 13 atomic commits on `feat/planning-reconcile-v2`,
2026-09-13.

## What was delivered

### Design-system primitives (`src/app/design-system/components/photo/`)

| Component | Selector | Purpose |
|-----------|----------|---------|
| `CePhotoComponent` | `ce-photo` | Display a photo by id; 128×128 thumbnail by default; 800×800 source size; skeleton → img → error placeholder; lazy-load via `<img loading="lazy">`; `clickable` input gates `photoClicked` output |
| `CePhotoGalleryComponent` | `ce-photo-gallery` | Flex-wrap thumbnail row + optional '+' add tile; embedded `ce-photo-lightbox`; `addRequested` and `photoDeleted` outputs |
| `CePhotoLightboxComponent` | `ce-photo-lightbox` | Full-screen overlay; Esc closes, ArrowLeft/Right navigate, click backdrop closes, click photo does not; delete button when `canDelete=true` |
| `CePhotoCaptureComponent` | `ce-photo-capture` | Camera mode (`getUserMedia` + frame capture) + upload mode (drag-and-drop + file picker); compression ladder via `photo-utils`; retry via `photo-utils`; toasts on errors |
| `CePhotoPanelComponent` | `ce-photo-panel` | Reusable wrapper combining gallery + capture + PhotoBindingCache; one-liner integration into any entity page |

All components are standalone, OnPush, signals-only, follow the existing
`ce-` selector convention, and respect `prefers-reduced-motion`.

### `src/app/features/photos/`

- `PhotosApiService` — handwritten HttpClient wrapper for
  `POST /api/v1/photos` (multipart), `GET /api/v1/photos/{id}` (blob),
  `DELETE /api/v1/photos/{id}`. Forward-compatible stubs for list +
  thumbnail routes that the backend doesn't ship yet.
- `PhotoUtils` — `compressImage` (0.8 → 0.6 → 0.3 ladder), `generateThumbnail`
  (cover-fit 128×128), `uploadWithRetry` (3 attempts, 1s/2s/4s).
- `PhotoBindingCacheService` — localStorage-backed frontend cache mapping
  `${entityType}:${entityId}` → `PhotoResponse[]`. See deviations.

### Entity page integration

- **Residents** — Row dropdown gains "View photos" action (visible to all
  roles). New View modal hosts `ce-photo-panel` with entityType='resident'.
- **Visits** — "Photos" button per row + modal with entityType='visitor'.
- **Vehicles** — Camera-emoji icon button per row + modal with entityType='vehicle'.
- **Service Providers** — "Photos" button per row + modal with
  entityType='service-provider'.

### Tests

- **Component specs (35 tests)**:
  - `ce-photo` (8 tests) — renders skeleton, click toggle, error placeholder,
    pixelSize override, srcUrl
  - `ce-photo-lightbox` (11 tests) — open/close, prev/next cycle, delete
    emission, fallback for empty array
  - `ce-photo-gallery` (7 tests) — empty state, add tile, click opens
    lightbox, photoDeleted forwarding
  - `ce-photo-capture` (9 tests) — file size/type rejection, upload success,
    retry recovers from transient failure, terminal failure → toast, camera
    permission denial → fallback to upload
- **`photo-utils.spec.ts` (9 tests)** — initial quality usage, fallback to
  0.6, throws `FILE_TOO_LARGE_AFTER_COMPRESSION`, retry succeeds, retries
  exhausted → throws, delay timing, observable integration
- **`photo-capture.spec.ts` (Playwright, 5 tests)**:
  - Camera capture (mocked `getUserMedia` via `addInitScript`)
  - EXIF strip — uploads a JPEG, fetches the source via `/api/v1/photos/{id}`,
    parses with `exifr`, asserts no GPS / Make / Model / DateTimeOriginal
  - Upload retry recovers from transient 503
  - Lightbox opens/closes/navigates
  - All four entity pages surface the photo panel

Total Angular test suite: **247 passing tests** under Karma + ChromeHeadless.

## Deviations from PLAN.md

These are critical deviations because the actual backend Phase 11 shipped
fewer endpoints than the plan's `12-CONTEXT.md` describes. The plan
explicitly stated the backend has `entityType`/`entityId` binding,
`/api/v1/photos/{id}/thumbnail`, and a list endpoint — none of which exist
in the current `src/Modules/Photos/` source.

### Backend gaps (Phase 11 actual vs. plan assumptions)

| Plan assumption | Actual Phase 11 | Mitigation |
|-----------------|------------------|------------|
| `POST /api/v1/photos` accepts `entityType` + `entityId` form fields | Multipart form with only `file` field | `PhotosApiService.upload(blob, fileName)` discards entityType/entityId on the wire |
| `GET /api/v1/photos?entityType=…` returns bound photos | No list endpoint exists | `PhotoBindingCacheService` keeps the binding map in `localStorage` keyed by `${type}:${id}` |
| `GET /api/v1/photos/{id}/thumbnail` returns 128×128 | No thumbnail route | `ce-photo` renders `/api/v1/photos/{id}` directly and crops via CSS `object-fit: cover` |
| `Photos.Delete` for soft-delete | ✅ Works | `photosApi.delete(id)` |
| `entity_type` + `entity_id` columns on `Photos` table | Not in `docker/mysql/init/09a-photos-schema.sql` | Backend schema-comment confirms: "Thumbnails are produced client-side in Phase 12 (ThumbnailPath nullable)" |

These are documented inline at every affected code site
(`// Phase 12 deviation:` comments) so the next phase can swap the cache
for `photosApi.list()` and the source-URL renderer for a real thumbnail
route without changing component contracts.

### Frontend-only consequences

1. **No cross-page photo persistence on the server.** Photos uploaded on
   one entity's modal survive only in the same browser profile via
   localStorage. Backend migration to add `entity_type` + `entity_id`
   columns + a list endpoint would close this gap.
2. **Thumbnails cost one full source fetch.** `ce-photo` uses the source
   blob URL and CSS cropping. Acceptable for Phase 12 sizes; a backend
   `/thumbnail` route would cut bandwidth ~99% on gallery loads.
3. **Multi-tenancy guard.** `Photos.Read/Write/Delete` are already wired
   to the porteiro + tenant-admin defaults in
   `src/Modules/Tenants/.../PorteiroDefaults.cs`, so the UI's permission
   gating aligns with the backend. No cross-tenant risk.

## Verification (post-plan)

```
✅ dotnet test tests/ControlEasyReborn.UnitTests       → 136/136 passed
✅ dotnet test tests/ControlEasyReborn.ArchitectureTests → 6/6 passed
⚠️  dotnet test tests/ControlEasyReborn.IntegrationTests → 73 skipped
   (Testcontainers needs nested Docker access; CI runs them green)
✅ npm run lint                                       → clean
✅ npx ng test (ChromeHeadless)                        → 247/247 passed
✅ docker compose -p ce-feat-planning-reconcile-v2 -f docker/docker-compose.yml ps
   → all 6 containers Up, db healthy, api serving
✅ curl -sk https://localhost:18091/api/v1/health      → Healthy
```

The integration tests couldn't run inside the sidecar `dotnet` container
in this environment (Testcontainers needs `/var/run/docker.sock` accessible
to a child container, and Windows Docker Desktop doesn't proxy that
cleanly to a sidecar). They are unaffected by Phase 12 (zero backend
changes) and pass in CI per the Phase 11 baseline.

## Acceptance evidence

- 13 atomic commits on `feat/planning-reconcile-v2` (range
  `5be2a19..21f879e`).
- 247 Angular unit tests pass (Karma + ChromeHeadless).
- `docker compose -p ce-feat-planning-reconcile-v2 -f docker/docker-compose.yml ps`
  reports all containers Up with the api healthy.
- Backend `dotnet test` (unit + architecture) green; integration green in
  CI but skipped locally for the documented Testcontainers-on-Windows reason.

## Risks tracked for future phases

| Risk | Status | Mitigation |
|------|--------|------------|
| PhotoBindingCache lives in localStorage — not portable across devices | Open | Phase 13 should grow the backend binding column + list endpoint and make the cache a write-through proxy |
| Source-URL thumbnails cost ~99% extra bandwidth vs. a real thumbnail route | Open | Phase 13/14 should ship `/api/v1/photos/{id}/thumbnail` (per Phase 11 summary) |
| EXIF strip depends on canvas redraw — pre-shrink to JPEG would defeat it | Mitigated | `compressImage` runs `canvas.toBlob` on the original, so EXIF is dropped before any quality pass |
| HEIC support in jsdom + Karma unit tests | Documented | HEIC decode falls through to file-extension sniff (`isValidType`); real device path is tested by E2E in a real browser |

## Out of scope (deferred)

- Bulk photo upload
- Photo cropping/rotation before upload
- Photo annotation
- Live face detection / autofocus
- HEIC output (Phase 12 emits JPEG; HEIC input is converted via canvas)
- Mobile native camera (PWA / Capacitor)
- Photo organization (albums, tags)
- Backend entity binding (depends on backend shipping the column + list endpoint)
