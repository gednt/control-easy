# Phase 12 UI Design Contract — Photo Capture & Display

**Phase:** 12 — Photo Capture & Display
**Status:** UI-SPEC Approved (smart discuss)
**Date:** 2026-09-12

---

## Visual Direction

This phase adds photo capture and display to the gatehouse workflow. The visual design follows the existing `mockup/` reference and uses the established `ce-*` design system. Photo capture is the moment of highest cognitive load for the porteiro (3-second workflow), so visual clarity and minimum-tap interaction take priority over visual flourish.

**Mood:** Functional, fast, neutral. The camera preview and capture button are the dominant elements. The rest of the chrome recedes.

---

## Surfaces

### 1. `ce-photo-capture` (Camera Mode Modal)

**Layout:**
- Full-width modal (`max-width: 480px` on mobile, `640px` on desktop)
- Header: title "Take photo" + close X button
- Body: live camera preview (`<video>` element with `aspect-ratio: 4/3`, `width: 100%`, `background: black`)
- Footer: bottom bar with three controls:
  - Left: "Cancel" text button
  - Center: large circular capture button (shutter icon, 64px diameter, primary color, raised shadow)
  - Right: "Switch camera" icon button (only shown if device has multiple cameras)
- Below capture: thin progress strip showing capture → compress → upload states
- Bottom right: small "Or upload from device" text link that switches to upload mode (kept brief — primary flow is camera)

**States:**
- Idle (camera loading) — spinner overlay on video frame; bottom bar disabled
- Live preview — capture button enabled
- Capturing (1-2s) — shutter flash overlay (white opacity 0.8 → 0 over 200ms)
- Compressing (200-500ms) — progress strip: "Compressing…" with animated dots
- Uploading (300-1500ms) — progress strip: "Uploading…" with retry counter if retrying
- Success — success toast, modal closes, gallery refreshes
- Permission denied — toast: "Camera access denied. Please allow camera access or upload a photo instead." + auto-switch to upload mode
- File too large after compression — toast: "Photo too large to upload. Try a smaller photo."
- Network failed after 3 retries — toast: "Upload failed. Try again?" with retry button

**Copy:**
- Modal title: "Take photo"
- Cancel: "Cancel"
- Switch camera: icon-only with `aria-label="Switch camera"`
- Capture button: `aria-label="Capture photo"`
- Upload fallback link: "Or upload from device"
- All errors via toast (not modal-blocking)

### 2. `ce-photo-capture` (Upload Mode Modal)

**Layout:**
- Same modal dimensions as camera mode
- Body: drag-and-drop zone (`min-height: 240px`, dashed border, "Drag photo here or click to browse" centered text)
- On file selected: thumbnail preview at top, "Remove" link below, "Upload" button at bottom

**States:**
- Idle (no file selected) — drag-and-drop zone visible
- File selected — preview shown; "Upload" button enabled
- Compressing — same as camera mode
- Uploading — same as camera mode
- All error states — same as camera mode

**Copy:**
- Modal title: "Upload photo"
- Drop zone: "Drag photo here or click to browse"
- Accepted types: "JPEG, PNG, HEIC (up to 8MB)"
- Remove: "Remove"
- Upload: "Upload"

### 3. `ce-photo` (Thumbnail Display)

**Layout:**
- 128×128 px square
- `border-radius: 4px`
- `object-fit: cover`
- Lazy-loaded via `<img loading="lazy">` with skeleton placeholder
- Cursor: pointer if `clickable`

**States:**
- Loading — skeleton shimmer (gray background with subtle pulse)
- Loaded — image displayed
- Error (broken URL) — placeholder with "?" icon (low-opacity)
- Click → opens lightbox (if `clickable`)

### 4. `ce-photo-gallery` (Multi-Photo Grid)

**Layout:**
- Horizontal flex row, `gap: 8px`, `flex-wrap: wrap`
- Up to 8 thumbnails visible; "+N more" badge if more
- First photo (most recent) shown at 128×128 with primary badge
- Subsequent photos at 96×96
- Right edge: small "+" tile (96×96) that opens the photo capture modal

**States:**
- Empty — single dashed-border "+ Add photo" tile
- Loading — skeleton placeholders for expected count
- Loaded — thumbnails rendered
- Error — empty state with retry button

### 5. `ce-photo-lightbox` (Full-Screen Photo View)

**Layout:**
- Full-viewport overlay (`position: fixed; inset: 0`)
- Background: `rgba(0, 0, 0, 0.9)`
- Center: source photo at `max-width: 800px`, `max-height: 80vh`
- Top right: close X button
- Left/right edges: prev/next chevrons (only if gallery has more than one photo)
- Bottom: caption strip with `recorded_at` timestamp in millisecond precision and soft-delete button (admin only)

**States:**
- Loading — spinner centered
- Loaded — photo + chrome
- Keyboard nav: Escape closes, ArrowLeft/Right navigate

---

## Typography

Uses existing design-system tokens:

- Modal title: `font-size: 18px`, `font-weight: 600`
- Body copy: `font-size: 14px`, `font-weight: 400`
- Button labels: `font-size: 14px`, `font-weight: 500`
- Caption text: `font-size: 12px`, `color: var(--ce-text-muted)`
- Error toasts: `font-size: 14px`, `color: var(--ce-error)`

No new fonts introduced. All text uses existing `font-sans` family.

---

## Color

Uses existing design-system tokens:

- Modal background: `var(--ce-surface)` (default surface)
- Primary action (capture button): `var(--ce-primary)`
- Capture button shadow: `var(--ce-primary) / 0.4` (40% opacity)
- Border (drop zone): `var(--ce-border)` dashed
- Lightbox background: `rgba(0, 0, 0, 0.9)`
- Skeleton placeholder: `var(--ce-surface-alt)` with opacity pulse animation

No new colors introduced. Dark theme auto-inherits from existing tokens.

---

## Spacing

- Modal padding: `var(--ce-space-4)` (16px)
- Thumbnail gap: `var(--ce-space-2)` (8px)
- Footer button gap: `var(--ce-space-3)` (12px)
- Lightbox padding: `var(--ce-space-2)` (8px) around photo

All spacing uses existing `ce-space-*` tokens.

---

## Motion

- Modal enter: fade + scale from 0.95 → 1 over 200ms (`ease-out`)
- Modal exit: fade + scale from 1 → 0.95 over 150ms
- Shutter flash: white overlay opacity 0.8 → 0 over 200ms
- Lightbox enter: fade over 200ms
- Lightbox image: fade in over 200ms (no scale)
- Skeleton pulse: opacity 1 → 0.6 → 1 over 1500ms (loop)
- Toast: slide up from bottom + fade over 200ms

All transitions use `ease-out`. Reduced-motion: respects `prefers-reduced-motion`, removes scale and slide.

---

## Design System Components Used

- `ce-modal` — host for capture modals
- `ce-button` — capture button, switch camera, cancel, upload
- `ce-icon` — close X, shutter, switch camera, chevrons
- `ce-spinner` — loading states
- `ce-toast` — error/success notifications
- `ce-skeleton` — thumbnail placeholders (new but trivial; can be inline if needed)

---

## Component Architecture

| Component | Selector | Inputs | Outputs |
|-----------|----------|--------|---------|
| PhotoCaptureComponent | `ce-photo-capture` | `{ entityType, entityId, mode: 'camera' \| 'upload' }` | `(photoUploaded)` |
| PhotoComponent | `ce-photo` | `{ photoId, size: 'thumbnail' \| 'source', clickable: boolean }` | `(click)` |
| PhotoGalleryComponent | `ce-photo-gallery` | `{ photos: Photo[], canAdd: boolean }` | `(addRequested)`, `(photoDeleted)` |
| PhotoLightboxComponent | `ce-photo-lightbox` | `{ photos: Photo[], startIndex: number }` | `(closed)` |

All components: standalone, OnPush, signals for state, `ce-` selector prefix.

---

## Integration Points

1. **Entity detail pages** — add `ce-photo-gallery` to header section (next to name + actions)
   - `features/residents/residents.page.ts`
   - `features/visits/` (visitor detail)
   - `features/vehicles/vehicles.page.ts`
   - `features/service-providers/` (detail page)

2. **Entity table rows** — replace generic avatar placeholder with `ce-photo` thumbnail column
   - `features/residents/residents.page.ts` — add photo column
   - `features/vehicles/vehicles.page.ts` — add photo column (if owner has photo)
   - Other entity tables: defer to follow-up

3. **Photos API service** — handwritten `PhotosApiService` in `features/photos/photos-api.service.ts`
   - Uses existing endpoints from Phase 11 (no backend changes)

---

## Copywriting Contract

| Context | Text | Notes |
|---------|------|-------|
| Camera modal title | "Take photo" | |
| Upload modal title | "Upload photo" | |
| Capture button | (icon only, aria-label "Capture photo") | |
| Cancel button | "Cancel" | |
| Switch camera button | (icon only, aria-label "Switch camera") | |
| Upload fallback link | "Or upload from device" | Toggles to upload mode |
| Drop zone | "Drag photo here or click to browse" | Upload mode |
| Accepted types note | "JPEG, PNG, HEIC (up to 8MB)" | Below drop zone |
| Remove file link | "Remove" | Upload mode preview |
| Upload button | "Upload" | Upload mode preview |
| Compression progress | "Compressing…" | With animated dots |
| Upload progress | "Uploading…" | With retry counter |
| Success toast | "Photo uploaded" | Auto-dismiss after 3s |
| Permission denied | "Camera access denied. Please allow camera access or upload a photo instead." | + auto-switch to upload |
| File too large | "Photo too large to upload. Try a smaller photo." | |
| Network failure | "Upload failed. Try again?" | With retry button |
| Wrong file type | "File type not supported. Use JPEG, PNG, or HEIC." | |
| Empty gallery | "No photos yet. Click + to add one." | Dashed border + tile |
| Gallery add tile | "+" icon, aria-label "Add photo" | |
| Lightbox close | (X icon, aria-label "Close") | |
| Lightbox prev/next | (chevron icons, aria-label "Previous photo" / "Next photo") | |
| Lightbox delete | "Delete" button | Admin/syndic only; confirm before delete |
| Lightbox caption | "{recorded_at} ms UTC" | Millisecond precision |

---

## Accessibility

- All buttons have `aria-label` (icon-only) or visible text
- Modal traps focus; returns focus to triggering button on close
- Live regions (`aria-live="polite"`) for upload progress
- Keyboard support: Escape closes modal/lightbox, ArrowLeft/Right navigate lightbox
- Reduced-motion preference respected (no scale/slide transitions)
- Color contrast: all text meets WCAG AA against background
- Screen reader announces capture/upload state changes

---

## Responsive

- Mobile (< 640px): full-screen modal; capture button 56px (still thumb-friendly)
- Tablet (640-1024px): centered modal, 480px width
- Desktop (> 1024px): centered modal, 640px width
- Lightbox always full-viewport on all sizes
- Gallery wraps to multiple rows on narrow viewports

---

## Performance

- Thumbnail lazy-load (`loading="lazy"`)
- Source images only loaded when lightbox opens
- Camera preview uses `requestVideoFrameCallback` if available; falls back to `requestAnimationFrame`
- Compression uses `OffscreenCanvas` if available; falls back to `<canvas>`
- Upload uses `fetch` with `signal` for cancellation if user closes modal mid-upload

---

## Open Questions for Implementer (agent's discretion)

- Exact modal dimensions and button positions
- Thumbnail skeleton vs blurhash placeholder
- Gallery layout: grid vs horizontal scroll
- Lightbox transition: fade vs slide
- Shutter flash intensity and duration
- EXIF metadata fields to verify in test (GPS only, or GPS + camera model + timestamp)

---

## Verification (per ROADMAP success criteria)

1. ✅ Camera capture flow under 5s on gatehouse WiFi (test with Playwright mocked getUserMedia)
2. ✅ 8MB iPhone HEIC → ≤500KB JPEG with thumbnail
3. ✅ EXIF stripped (Playwright test verifies with exifr)
4. ✅ 3 retries with backoff (Playwright test simulates network failure)
5. ✅ 128×128 thumbnail in table row; click opens lightbox
6. ✅ Photos display on all four entity record pages
7. ✅ Playwright E2E: camera, upload, photo display
8. ✅ `dotnet test` + `npm test` green; Docker stack healthy

---

## UI Considerations

**UI element states to cover** (per ui-consideration-probe axes):

- **Capture modal:** all 8 state transitions (idle, live preview, capturing, compressing, uploading, success, permission denied, file too large, network failure)
- **Upload modal:** all 5 state transitions (idle, file selected, compressing, uploading, error)
- **Thumbnail:** loading, loaded, error
- **Gallery:** empty, loading, loaded, error
- **Lightbox:** loading, loaded, navigation, close
- **Camera permission flow:** denied → fallback to upload (handled via toast + auto-switch)
- **EXIF verification:** automated test asserts no GPS/camera-model/timestamp in uploaded blob

All states have explicit copy in the copywriting contract above. Implementation must render all states (not just happy path).
