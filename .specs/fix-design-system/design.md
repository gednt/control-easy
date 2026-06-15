# Design: Fix Design System Inconsistencies

## Overview

The ControlEasy Reborn design system has a solid foundation — tokens, theme service, and a complete component library — but suffers from significant inconsistencies that undermine its value as a single source of truth. The most critical issue is the `--space-*` vs `--spacing-*` token split: design system components use `--space-*` (defined in `tokens/spacing.css`) while layout and feature components use `--spacing-*` (mapped in `styles.css @theme` block). This affects **15+ files** across the entire codebase, not just the 4 files originally identified.

The second major issue is style duplication. Not just the login page and dashboard, but **every feature page** copies design system component CSS rather than importing components. The current fix scope covers login and dashboard; remaining pages will be a follow-up spec.

The `@theme` self-reference pattern (`--color-background: var(--color-background)`) is **valid Tailwind v4 behavior** — it exposes custom CSS properties for utility class generation. The `:root` declarations provide runtime values, and `@theme` declarations tell Tailwind about the tokens. The real issue is the naming inconsistency, not the self-references.

Additionally, several components (modal, dropdown, tooltip) import Angular CDK Overlay but never use it correctly, resulting in dead code or broken positioning.

## Glossary

- **Token** — A CSS custom property defined in `tokens/*.css` and consumed via `var(--token-name)`.
- **`--space-*`** — The canonical spacing scale defined in `tokens/spacing.css` (e.g., `--space-1: 4px`).
- **`--spacing-*`** — A secondary spacing scale mapped in `styles.css @theme` block. Must be kept as Tailwind utility aliases, but component CSS should use `--space-*` exclusively.
- **`@theme` directive** — Tailwind v4's mechanism for injecting custom values into Tailwind's theme system. Self-references like `--color-background: var(--color-background)` are intentional — they expose custom properties to Tailwind for utility class generation.
- **`--color-text-on-primary`** — New token for text placed on primary-colored backgrounds (replaces hardcoded `color: white`). White in light mode, white in dark mode (primary is already lighter).
- **`--color-accent-pink`** — New token for the pink accent color used in avatar gradients (replaces hardcoded `#ec4899`).
- **CDK Overlay** — Angular CDK's overlay system for rendering floating UI (modals, dropdowns, tooltips) in a portal outside the component tree.
- **`ce-` prefix** — Selector prefix for all design system components.
- **Dark theme** — Applied via `[data-theme="dark"]` attribute on `<html>`, managed by `ThemeService`.

## Architecture

### Token Unification Strategy

The fix centers on making `--space-*` the only spacing token namespace used in component CSS, while keeping `--spacing-*` in the `@theme` block as Tailwind utility aliases only.

**Current flow:**
```
tokens/spacing.css → :root { --space-1: 4px }
styles.css @theme  → --spacing-1: var(--space-1)  // ← Tailwind utility alias
design components  → var(--space-3)               // ← canonical name
layout/feature     → var(--spacing-3)             // ← wrong name (15+ files)
```

**Target flow:**
```
tokens/spacing.css → :root { --space-1: 4px }
styles.css @theme  → --spacing-1: var(--space-1)  // ← kept for Tailwind utilities
ALL components      → var(--space-3)               // ← single canonical name
```

The most efficient approach is a project-wide find-and-replace of `var(--spacing-` to `var(--space-` across all `.ts` files in `src/Web/ControlEasyReborn.Web/src/app/`, then verify no `var(--spacing-` references remain.

**Color token unification:**
```
colors.css :root   → --sidebar-bg: #0f172a        // ← no --color- prefix
styles.css @theme  → --color-sidebar-bg: var(--sidebar-bg)  // ← indirect
```

Target: Define sidebar colors with `--color-` prefix directly in `colors.css`, remove the intermediate `--sidebar-*` aliases.

```css
/* colors.css :root — BEFORE */
--sidebar-bg: #0f172a;
--sidebar-text: #cbd5e1;

/* colors.css :root — AFTER */
--color-sidebar-bg: #0f172a;
--color-sidebar-text: #cbd5e1;
```

And in `styles.css @theme`, remove the `--color-sidebar-*: var(--sidebar-*)` mappings since the tokens now have the `--color-` prefix directly.

**Note:** `--sidebar-width` and `--sidebar-width-collapsed` in `styles.css:101-102` are layout dimension tokens, NOT color tokens. They should remain unchanged.

**New color tokens:**
```css
/* colors.css :root */
--color-text-on-primary: #ffffff;
--color-accent-pink: #ec4899;

/* colors.css [data-theme="dark"] */
--color-text-on-primary: #ffffff;  /* stays white since primary is already lighter */
--color-accent-pink: #f472b6;
```

### Component Consumption Strategy

Pages must import and use design system components instead of duplicating their styles:

- **Login page:** Replace `.ce-button`, `.ce-input-group/wrapper/label/input/error`, `.ce-checkbox-box`, `.ce-spinner` CSS with `<ce-button>`, `<ce-input>`, `<ce-checkbox>`, `<ce-spinner>` component usage.
- **Change-password page:** Same treatment as login page — replace duplicated component CSS with design system component imports.
- **Dashboard:** Replace inline `.ce-card` CSS with `<ce-card>` component usage.

**Out of scope (follow-up spec):** The remaining 7+ feature pages that duplicate design system styles (residents, visits, vehicles, apartments, service-providers, administration, condominiums). The current fix addresses the `color: white` and `--spacing-*` instances in these pages but does not refactor them to use components.

### Component Fix Strategy

**Modal (`CeModalComponent`):**
- Remove dead CDK Overlay code (imports, overlay injection, `openModal`, `closeModal`, `overlayRef`, `ViewChild`).
- The current `@if` template approach is fine for v1 — it's simpler and works. Keep it.
- Add `(document:keydown.escape)` host event to call `close()`.
- Add focus management: focus the modal on open, restore focus on close.

**Dropdown (`CeDropdownComponent`):**
- The `.ce-dropdown` wrapper already has `position: relative` (line 21). The actual issue is that `.ce-dropdown-panel` is rendered as a **sibling** of `.ce-dropdown` (via `@if` block), not a child. This means `position: absolute` on the panel positions relative to the host element, not the `.ce-dropdown` wrapper.
- Fix: Restructure template so the panel is inside `.ce-dropdown`, or use `host: { style: 'position: relative' }` on the host element.
- Remove unused CDK Overlay imports (`OverlayModule`, `Overlay`, `OverlayRef`, `NgTemplateOutlet`, `NgClass`).

**Tooltip (`CeTooltipDirective`):**
- Remove the `prefers-reduced-motion: reduce` guard that prevents tooltips from showing entirely. Tooltips are informational, not decorative — they should always be accessible regardless of motion preferences.
- Add viewport edge clamping to tooltip positioning (check if tooltip would overflow right/bottom edges and adjust).
- Remove unused `Overlay`, `FlexibleConnectedPositionStrategy`, `TemplateRef` imports.

**Tabs (`CeTabsComponent`):**
- Current `ng-content` projects all `CeTabComponent` children at once with no selective rendering.
- Fix: Restructure `CeTabComponent` to accept a `content` template input, then use `ContentChildren(CeTabComponent)` in `CeTabsComponent` to conditionally render only the active tab's content.
- Implementation:
  ```typescript
  // CeTabComponent
  content = input.required<TemplateRef<unknown>>();

  // CeTabsComponent — template
  @for (tab of tabs(); track tab.label()) {
    @if ($index === activeIndex()) {
      <ng-container *ngTemplateOutlet="tab.content()"></ng-container>
    }
  }
  ```

**Breadcrumbs (`CeBreadcrumbsComponent`):**
- Replace `[href]` with `[routerLink]` for SPA navigation.
- Add `RouterLink` and `RouterLinkActive` imports.

**Hardcoded `white` color:**
- Add `--color-text-on-primary: #ffffff` to `:root` and `[data-theme="dark"]` tokens.
- Replace all 29 `color: white` instances with `var(--color-text-on-primary)` or `var(--color-text-inverse)`.
- In design system components: replace directly.
- In feature pages: replace `color: white` instances; most will be eliminated when pages are refactored to use components (follow-up spec).

**Hardcoded `#ec4899`:**
- Add `--color-accent-pink: #ec4899` token (`--color-accent-pink-light: #f472b6` for gradient endpoints).
- Replace in sidebar, topbar, and residents page.

### Sidebar Dark Theme

Add `[data-theme="dark"]` overrides for sidebar tokens:
```css
[data-theme="dark"] {
  --color-sidebar-hover-bg: #334155;
  --color-sidebar-border: #334155;
}
```
The sidebar background and active colors remain the same (dark sidebar in both themes is a common pattern), but hover states should lighten slightly in dark mode.

## UI/UX Specification

### Component Hierarchy (unchanged)
No new components are introduced. All changes are fixes to existing components.

### Layout and Responsive Behavior
No layout changes. The fix preserves all existing responsive breakpoints and layout patterns.

### Interaction Patterns
- **Modal:** Add Escape key handler via `(document:keydown.escape)`. Add focus management (focus modal on open, restore focus on close).
- **Dropdown:** Restructure template so panel is inside the positioned wrapper, or position relative to host.
- **Tooltip:** Remove `prefers-reduced-motion` guard so tooltips always show. Add viewport edge clamping.
- **Tabs:** Show only the active tab's content via `ContentChildren` + conditional template rendering.

### Accessibility
- Checkbox focus ring: `.ce-checkbox-input:focus-visible + .ce-checkbox-box` is correct, widely-supported accessibility practice. The login page duplication of checkbox styles uses an `::after` pseudo-element for the checkmark — these are complementary, not conflicting.
- Breadcrumbs: Use `routerLink` for SPA navigation, maintaining aria-current for the active crumb.
- Modal: Add Escape key handler and focus trap.

### Animation and Transition
No animation changes. All existing transitions and keyframes are preserved.

## Files to Modify

1. `src/Web/ControlEasyReborn.Web/src/styles.css` — Add `--color-text-on-primary` and `--color-accent-pink` to `@theme` block; rename sidebar token mappings from `var(--sidebar-*)` to `var(--color-sidebar-*)`; replace `color: white` in `.skip-link`; add comment clarifying `--spacing-*` aliases are Tailwind-only.
2. `src/Web/ControlEasyReborn.Web/src/app/design-system/tokens/colors.css` — Add `--color-text-on-primary`, `--color-accent-pink`, `--color-accent-pink-light`; rename `--sidebar-*` to `--color-sidebar-*`; add dark mode sidebar overrides.
3. `src/Web/ControlEasyReborn.Web/src/app/design-system/tokens/spacing.css` — No changes (canonical source).
4. `src/Web/ControlEasyReborn.Web/src/app/design-system/components/button/button.component.ts` — Replace `color: white` on primary and danger variants with `var(--color-text-on-primary)`.
5. `src/Web/ControlEasyReborn.Web/src/app/design-system/components/modal/modal.component.ts` — Remove dead CDK Overlay code, add Escape key handler, add focus management.
6. `src/Web/ControlEasyReborn.Web/src/app/design-system/components/dropdown/dropdown.component.ts` — Restructure template for correct positioning, remove unused CDK imports.
7. `src/Web/ControlEasyReborn.Web/src/app/design-system/components/tooltip/tooltip.directive.ts` — Remove `prefers-reduced-motion` guard, add viewport clamping, remove unused imports.
8. `src/Web/ControlEasyReborn.Web/src/app/design-system/components/tabs/tabs.component.ts` — Restructure `CeTabComponent` to accept content template, add `ContentChildren` + conditional rendering.
9. `src/Web/ControlEasyReborn.Web/src/app/design-system/components/breadcrumbs/breadcrumbs.component.ts` — Replace `[href]` with `[routerLink]`.
10. `src/Web/ControlEasyReborn.Web/src/app/design-system/components/empty-state/empty-state.component.ts` — Replace `color: white` with `var(--color-text-on-primary)`.
11. `src/Web/ControlEasyReborn.Web/src/app/design-system/components/pagination/pagination.component.ts` — Replace `color: white` with `var(--color-text-on-primary)`.
12. `src/Web/ControlEasyReborn.Web/src/app/design-system/components/avatar/avatar.component.ts` — Replace `color: white` with `var(--color-text-on-primary)`.
13. All files using `--spacing-*` tokens — Replace `var(--spacing-*)` with `var(--space-*)`. This includes: sidebar, topbar, login, dashboard, app-shell, apartment-picker, demo-banner, demo-help, and all feature pages.
14. `src/Web/ControlEasyReborn.Web/src/app/layout/sidebar/sidebar.component.ts` — Replace `color: white` with `var(--color-text-on-primary)`, replace `#ec4899` with `var(--color-accent-pink)`, replace `var(--sidebar-*)` with `var(--color-sidebar-*)` (already using correct names — verify only).
15. `src/Web/ControlEasyReborn.Web/src/app/layout/topbar/topbar.component.ts` — Replace `color: white` with `var(--color-text-on-primary)`, replace `#ec4899` with `var(--color-accent-pink)`.
16. `src/Web/ControlEasyReborn.Web/src/app/features/auth/login.page.ts` — Replace duplicated component CSS with design system component usage, replace remaining `color: white` with token.
17. `src/Web/ControlEasyReborn.Web/src/app/features/auth/change-password.page.ts` — Replace duplicated component CSS with design system component usage, replace remaining `color: white` with token.
18. `src/Web/ControlEasyReborn.Web/src/app/features/dashboard/dashboard.page.ts` — Replace inline `.ce-card` CSS with `<ce-card>` usage, replace inline `var(--spacing-*)` with `var(--space-*)`.
19. All remaining feature pages — Replace `var(--spacing-*)` with `var(--space-*)`, replace `color: white` with `var(--color-text-on-primary)`, replace `#ec4899` with `var(--color-accent-pink)`.

## Testing Strategy

1. **Visual regression:** After each component fix, verify rendering in the showcase page (`/design-system/showcase`) in both light and dark themes.
2. **Token consistency:** Grep for all `var(--spacing-` usage across the project. After the fix, there should be zero results in component/layout/feature CSS (only in `styles.css @theme` block).
3. **Token consistency:** Grep for `color: white` and `color: #fff`. After the fix, there should be zero results in component/layout/feature CSS (only `colors.css` token definitions and non-token contexts).
4. **Functional tests:**
   - Modal opens and closes with Escape key and backdrop click.
   - Dropdown positions correctly relative to its trigger.
   - Tooltip shows on hover regardless of `prefers-reduced-motion`.
   - Tabs show only active tab content.
   - Breadcrumbs navigate via Angular router (no full page reload).
5. **Dark theme:** Toggle theme and verify all components render correctly, including sidebar hover states.

## Verification Approach

1. `grep -r 'var(--spacing-' src/Web/ControlEasyReborn.Web/src/app/` should return zero results (only `styles.css @theme` block should have `--spacing-*`).
2. `grep -rn 'color: white' src/Web/ControlEasyReborn.Web/src/app/` should return zero results.
3. `grep -rn '#ec4899' src/Web/ControlEasyReborn.Web/src/app/` should return zero results.
4. `ng build` should succeed with no errors.
5. Showcase page renders correctly in light and dark themes.
6. Login page and change-password page use `<ce-button>`, `<ce-input>`, `<ce-checkbox>` instead of duplicated CSS.
7. Dashboard page uses `<ce-card>` instead of inline card styles.

## Out of Scope (Follow-up Spec)

- Refactor all feature pages (residents, visits, vehicles, apartments, service-providers, administration, condominiums) to use design system components instead of duplicating component CSS.
- Consider `--color-text-on-primary` dark mode value — some dark mode designs use `rgba(0,0,0,0.87)` for text on primary-colored backgrounds.
- Migrate tooltip to CDK Overlay for robust positioning.
- Migrate dropdown to CDK Overlay for robust positioning.
- Migrate modal to CDK Overlay for proper portal rendering.