# Requirements: Fix Design System Inconsistencies

## UC1: Consistent CSS custom property naming

As a developer, I need all CSS custom properties to follow a single naming convention throughout the design system and layout components, so that token usage is predictable and not dependent on whether the file is a design-system component or a layout/feature component.

**Current problem:** Design system components use `--space-*` (e.g., `var(--space-3)`), while layout and feature components use `--spacing-*` (e.g., `var(--spacing-3)`). Two divergent namespaces exist for the same values. This is the most pervasive issue — it affects not just 4 files but **15+ files** across the entire codebase: sidebar, topbar, login, dashboard, app-shell, apartment-picker, demo-banner, demo-help, and all feature pages (residents, visits, vehicles, apartments, service-providers, administration, condominiums, change-password). The design system token files only define `--space-*`, so the `--spacing-*` usages rely on the `@theme` block aliases in `styles.css`.

## UC2: Tailwind `@theme` bridge correctly exposes custom tokens

As a developer, I need the `@theme` block in `styles.css` to correctly expose custom tokens to Tailwind's utility class system, so that tokens like `bg-background`, `p-spacing-3`, etc. generate working utility classes.

**Current problem:** The `@theme` block in `styles.css` (lines 9–98) declares tokens like `--color-background: var(--color-background)`, `--spacing-1: var(--space-1)`, etc. This pattern is **valid Tailwind v4 behavior** — `@theme` entries with `var()` references tell Tailwind about custom CSS properties for utility generation, while `:root` and `[data-theme="dark"]` provide the runtime values. The real issue is the naming inconsistency: `--spacing-*` aliases in `@theme` vs `--space-*` in `:root`, causing developers to use both interchangeably. The `@theme` block should keep `--spacing-*` as Tailwind utility aliases (so `p-spacing-3` works), but all component CSS should exclusively use `--space-*`.

## UC3: Design system components should be consumed, not duplicated

As a developer, I need layout and feature pages to import and use design system components rather than copy-pasting their styles, so that visual changes propagate from a single source of truth.

**Current problem:** Multiple pages duplicate design system component CSS instead of importing components:
- `login.page.ts` — duplicates ~200 lines: `.ce-button`, `.ce-input-group`, `.ce-input-label`, `.ce-input-wrapper`, `.ce-checkbox`, `.ce-spinner`
- `change-password.page.ts` — duplicates the same set: `.ce-button`, `.ce-input-group`, `.ce-input-label`, `.ce-input`, `.ce-input-error`, `.login-page`, `.login-card`, `.login-brand`, `.login-brand-mark`
- `dashboard.page.ts` — duplicates `.ce-card` and `.ce-card.accent-primary`
- `residents.page.ts` — duplicates `.ce-button`, `.ce-table`, `.ce-badge`, `.ce-input-group`, `.ce-checkbox`, `.ce-empty-state`, `.ce-card`
- `apartments.page.ts` — duplicates `.ce-button`, `.ce-table`, `.ce-badge`, `.ce-input-group`, `.ce-card`
- `visits.page.ts` — duplicates `.ce-button`, `.ce-table`, `.ce-input-group`, `.ce-badge`
- `vehicles.page.ts` — duplicates `.ce-button`, `.ce-table`, `.ce-badge`, `.ce-input-group`
- `service-providers.page.ts` — duplicates `.ce-button`, `.ce-input-group`
- `administration.page.ts` — duplicates `.ce-button`, `.ce-table`, `.ce-input-group`
- `condominiums.page.ts` — duplicates `.ce-button`, `.ce-input-group`, `.ce-card`

**Scope note:** The current fix covers login and dashboard pages. The remaining feature pages will be addressed in a follow-up spec.

## UC4: Sidebar color tokens should use consistent naming

As a developer, I need sidebar color tokens to follow the same `--color-*` prefix as all other design system tokens, so that theming behavior is predictable and discoverable.

**Current problem:** The `colors.css` token file defines sidebar colors as `--sidebar-bg`, `--sidebar-text`, etc. (without the `--color-` prefix). The `@theme` block in `styles.css` then maps them as `--color-sidebar-bg: var(--sidebar-bg)`. This creates an inconsistent two-hop indirection where some semantic colors are directly `--color-*` and others require `--sidebar-* → --color-sidebar-*`. Note: `--sidebar-width` and `--sidebar-width-collapsed` in `styles.css:101-102` are layout dimension tokens, NOT color tokens, and should remain unchanged.

## UC5: Remove hardcoded `color: white` and magic color values

As a developer, I need all color values in component styles to reference design tokens, so that dark theme switching works correctly and the color palette is centralized.

**Current problem:** There are **29 instances** of hardcoded `color: white` across 15 files, plus 3 instances of hardcoded `#ec4899`:

**Design system components (6 instances):**
- `button.component.ts:51` — `color: white` on primary variant
- `button.component.ts:72` — `color: white` on danger variant
- `empty-state.component.ts:57` — `color: white` on action button
- `pagination.component.ts:57` — `color: white` on active page
- `avatar.component.ts:24` — `color: white` on initials

**Layout components (4 instances):**
- `sidebar.component.ts:119` — `color: white` on brand text
- `sidebar.component.ts:161` — `color: white` on active nav items
- `sidebar.component.ts:183` — hardcoded `#ec4899` gradient color
- `topbar.component.ts:195` — `color: white` on avatar, plus `#ec4899`

**Global styles (2 instances):**
- `styles.css:179,185` — `color: white` on `.skip-link`

**Feature pages (17+ instances — addressed via component refactoring and follow-up spec):**
- `login.page.ts:195` — `color: white` on brand mark
- `change-password.page.ts:111,175` — duplicated button/brand styles
- `service-providers.page.ts:75` — duplicated `.variant-primary`
- `administration.page.ts:90,96` — `.tab.active` and `.variant-primary`
- `vehicles.page.ts:103` — `.variant-primary`
- `apartments.page.ts:232,235` — `.variant-primary` and `.variant-danger`
- `condominiums.page.ts:343` — `.variant-primary`
- `visits.page.ts:138,161` — `.filter-tab.active` and `.variant-primary`
- `residents.page.ts:332,338,434` — `.variant-primary`, `.variant-danger`, `.resident-avatar` (with `#ec4899`)

These should use `var(--color-text-on-primary)` or `var(--color-text-inverse)`.

## UC6: Sidebar tokens should be theme-aware

As a developer, I need the sidebar to respond to dark/light theme changes so that the application feels cohesive when switching themes.

**Current problem:** The sidebar tokens (`--sidebar-bg`, `--sidebar-text`, etc.) are defined only in `:root` (light mode). The `[data-theme="dark"]` block in `colors.css` does not override sidebar colors. The sidebar will look the same in both themes, which is intentional for a "dark sidebar" pattern, but the sidebar hover/active tokens should still adapt for better contrast in dark mode.

## UC7: Modal component uses Angular CDK Overlay incorrectly

As a developer, I need the modal to actually render in an overlay, so that it functions as a proper dialog.

**Current problem:** The `CeModalComponent` imports `OverlayModule` and `Overlay` but its `openModal()` method creates an overlay ref that never attaches the template. The component instead uses a manual `@if` pattern in the template. The CDK overlay code (lines 128-133) is dead code — the overlay ref is created but never used to display content.

## UC8: Dropdown component template structure prevents correct positioning

As a developer, I need the dropdown to position correctly relative to its trigger element, so that menus appear where expected.

**Current problem:** The `CeDropdownComponent` template renders `.ce-dropdown-panel` as a sibling of `.ce-dropdown` (via `@if` block), not as a child. While `.ce-dropdown` has `position: relative` (line 21), the `.ce-dropdown-panel` is not inside it, so `position: absolute` on the panel positions relative to the nearest positioned ancestor (the host element), not the `.ce-dropdown` wrapper. The component also imports `OverlayModule` and `Overlay` but never uses them.

## UC9: Tooltip directive bypasses Angular rendering entirely

As a developer, I need the tooltip to be rendered within Angular's change detection and overlay system, so that it positions correctly and doesn't cause memory leaks or z-index conflicts.

**Current problem:** `CeTooltipDirective` creates tooltip elements using `Renderer2` and appends them directly to `document.body`, bypassing Angular's component tree entirely. It also doesn't use the injected `Overlay` service. Positioning is based on `getBoundingClientRect()` and manual pixel calculations, which won't handle scrolling, overflow, or viewport edge cases. The directive also skips showing tooltips entirely when `prefers-reduced-motion: reduce` is active (line 27), conflating motion preference with tooltip accessibility.

## UC10: Tab component content projection is incomplete

As a developer, I need tabs to show/hide their projected content based on the active tab index, so that tab navigation actually works.

**Current problem:** `CeTabsComponent` renders tab buttons correctly but has no mechanism to show/hide the projected `CeTabComponent` content based on `activeIndex`. The `ng-content` projects all `CeTabComponent` children at once with no selective rendering. Simple `@if` or `[style.display]` won't work with `ng-content`. This requires restructuring `CeTabComponent` to accept content templates and using `ContentChildren` query + conditional rendering based on `activeIndex()`.

## UC11: Checkbox focus ring relies on CSS sibling selector with hidden input

As a developer, I need the checkbox focus ring to display reliably for keyboard users, so that accessibility compliance is met.

**Current problem:** The checkbox component in `checkbox.component.ts:81` uses `.ce-checkbox-input:focus-visible + .ce-checkbox-box`, where `.ce-checkbox-input` has `opacity: 0` and `pointer-events: none`. While `:focus-visible` should still fire on a focused hidden input, this is fragile across browsers. The login page duplicates the checkbox pattern with an additional `::after` pseudo-element for the checkmark styling (not a conflicting focus ring — these are complementary styles).

## UC12: Breadcrumbs component uses `<a href>` instead of `routerLink`

As a developer, I need the breadcrumbs to use Angular router navigation so that page transitions are client-side and don't trigger full page reloads.

**Current problem:** `CeBreadcrumbsComponent` uses `[href]="crumb.route ?? '/'"` instead of `[routerLink]`, causing full browser navigations instead of SPA routing.