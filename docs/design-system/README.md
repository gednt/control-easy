# ControlEasy Reborn — Design System

> The visual design system for the ControlEasy Reborn Angular SPA.

## Quick links

- **Showcase page:** `/design-system/showcase` (live component gallery, both themes)
- **Specs:** [requirements.md](../../.specs/2%20-%20visual-design-system/requirements.md) | [design.md](../../.specs/2%20-%20visual-design-system/design.md) | [tasks.md](../../.specs/2%20-%20visual-design-system/tasks.md)
- **Source:** `src/Web/ControlEasyReborn.Web/src/app/design-system/`

## Architecture

The system follows a **token → theme → component** pipeline:

1. **Tokens** — CSS custom properties in `src/app/design-system/tokens/` (colors, radii, spacing, typography, shadows, motion)
2. **Theme** — `ThemeService` toggles `light`/`dark`/`system` via `data-theme` attribute on `<html>`
3. **Components** — Standalone Angular components in `src/app/design-system/components/`, consuming tokens via Tailwind utilities and `var(--*)`

## How to add a new base component

1. Create `src/app/design-system/components/<name>/<name>.component.ts`
2. Use the `ce-` selector prefix (e.g. `ce-button`)
3. Set `standalone: true`, `changeDetection: ChangeDetectionStrategy.OnPush`
4. Use only design tokens — no hex literals in templates, only `var(--color-*)` or Tailwind utility classes
5. Add ARIA attributes (`role`, `aria-label`, `aria-*`) for accessibility
6. Add `prefers-reduced-motion` respect if animated
7. Export from `src/app/design-system/index.ts`
8. Add the component to the showcase page

## Component inventory

| Selector | Purpose | File |
|---|---|---|
| `ce-button` | Button with variant, size, loading, disabled | `components/button/` |
| `ce-card` | Card container with accent, padded, slots | `components/card/` |
| `ce-input` | Form input with label, error, CVA | `components/input/` |
| `ce-stat-tile` | Stat display with trend chip | `components/stat-tile/` |
| `ce-badge` | Status badge with tone and size | `components/badge/` |
| `ce-modal` | Modal dialog (manual implementation) | `components/modal/` |
| `ce-toast-host` + `ToastService` | Toast notification system | `components/toast/` |
| `ce-table` | Table wrapper with sticky header | `components/table/` |
| `ce-empty-state` | Empty state with icon, CTA | `components/empty-state/` |
| `ce-spinner` | Loading spinner | `components/spinner/` |
| `ce-avatar` | Avatar with initials fallback | `components/avatar/` |
| `ce-tabs` + `ce-tab` | Tab navigation | `components/tabs/` |
| `ce-dropdown` | Dropdown menu | `components/dropdown/` |
| `[ceTooltip]` | Tooltip directive | `components/tooltip/` |
| `ce-pagination` | Pagination with page size selector | `components/pagination/` |
| `ce-breadcrumbs` | Breadcrumb navigation | `components/breadcrumbs/` |
| `ce-checkbox` | Checkbox with label, indeterminate | `components/checkbox/` |

## Theming

- Toggle theme: inject `ThemeService` and call `toggle()` or `setTheme('light' | 'dark' | 'system')`
- Theme persists in `localStorage["ce.theme"]`
- FOUC prevention: inline `<script>` in `index.html` reads localStorage before paint
- Dark mode: `[data-theme="dark"]` overrides CSS custom properties

## Brand customization hook

The `--color-primary-raw` variable is the single-point brand override:

```css
[data-tenant="acme"] {
  --color-primary-raw: #059669;
}
```

All `--color-primary` references inherit automatically. A future `TenantBrandService` will set `data-tenant` on `<app-root>`.

## Do / Don't

| Do | Don't |
|---|---|
| Use `var(--color-primary)` or `bg-primary` | Hardcode `#4f46e5` in templates |
| Use `var(--radius-lg)` or `rounded-lg` | Write `border-radius: 12px` inline |
| Use `ThemeService.toggle()` | Manipulate `data-theme` directly |
| Use `ce-button variant="primary"` | Create a `<button class="bg-primary">` |
| Use `$localize` for all user strings | Hardcode English/Portuguese strings |