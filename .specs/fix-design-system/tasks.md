# Tasks: Fix Design System Inconsistencies

- [x] 1. **Unify spacing tokens: replace `--spacing-*` with `--space-*` across all files**
  - [x] 1.1 Project-wide find-and-replace: `var(--spacing-` → `var(--space-` in all `.ts` files under `src/Web/ControlEasyReborn.Web/src/app/`
  - [x] 1.2 Verify the following files were updated (known `--spacing-*` consumers):
    - `sidebar.component.ts`
    - `topbar.component.ts`
    - `login.page.ts`
    - `dashboard.page.ts` (including inline styles in template)
    - `app-shell.component.ts`
    - `apartment-picker.component.ts`
    - `demo-banner.component.ts`
    - `demo-help.page.ts`
    - `residents.page.ts`
    - `visits.page.ts`
    - `vehicles.page.ts`
    - `apartments.page.ts`
    - `service-providers.page.ts`
    - `administration.page.ts`
    - `condominiums.page.ts`
    - `change-password.page.ts`
  - [x] 1.3 In `styles.css`, add comment clarifying that `--spacing-*` aliases in `@theme` are Tailwind utility generators only — component CSS must use `--space-*`
  - [x] 1.4 Run `grep -rn 'var(--spacing-' src/Web/ControlEasyReborn.Web/src/app/` and confirm zero results

- [x] 2. **Add missing color tokens and unify sidebar token naming**
  - [x] 2.1 In `colors.css`, rename `--sidebar-bg` → `--color-sidebar-bg`, `--sidebar-text` → `--color-sidebar-text`, `--sidebar-text-muted` → `--color-sidebar-text-muted`, `--sidebar-hover-bg` → `--color-sidebar-hover-bg`, `--sidebar-active-bg` → `--color-sidebar-active-bg`, `--sidebar-active-text` → `--color-sidebar-active-text`, `--sidebar-border` → `--color-sidebar-border` (in both `:root` and `[data-theme="dark"]`)
  - [x] 2.2 In `colors.css`, add to `:root`:
    ```css
    --color-text-on-primary: #ffffff;
    --color-accent-pink: #ec4899;
    --color-accent-pink-light: #f472b6;
    ```
  - [x] 2.3 In `colors.css`, add to `[data-theme="dark"]`:
    ```css
    --color-text-on-primary: #ffffff;
    --color-accent-pink: #f472b6;
    --color-accent-pink-light: #f9a8d4;
    --color-sidebar-hover-bg: #334155;
    --color-sidebar-border: #334155;
    ```
  - [x] 2.4 Update `styles.css` `@theme` block: replace `var(--sidebar-*)` references with `var(--color-sidebar-*)`, add `--color-text-on-primary` and `--color-accent-pink` and `--color-accent-pink-light` entries
  - [x] 2.5 Verify sidebar and topbar components already use `var(--color-sidebar-*)` (they should be, since `@theme` mapped them). Confirm no `var(--sidebar-*` references remain in component CSS.
  - [x] 2.6 Note: `--sidebar-width` and `--sidebar-width-collapsed` in `styles.css:101-102` are layout dimension tokens and should remain unchanged.

- [x] 3. **Replace hardcoded `color: white` and `#ec4899` with design tokens**
  - [x] 3.1 `button.component.ts` — replace `color: white` on primary variant (line 51) and danger variant (line 72) with `color: var(--color-text-on-primary)`
  - [x] 3.2 `empty-state.component.ts` — replace `color: white` on action button (line 57) with `color: var(--color-text-on-primary)`
  - [x] 3.3 `pagination.component.ts` — replace `color: white` on active page (line 57) with `color: var(--color-text-on-primary)`
  - [x] 3.4 `avatar.component.ts` — replace `color: white` on initials (line 24) with `color: var(--color-text-on-primary)`
  - [x] 3.5 `sidebar.component.ts` — replace `color: white` (lines 119, 161) with `color: var(--color-text-on-primary)`, replace `#ec4899` (line 183) with `var(--color-accent-pink)`
  - [x] 3.6 `topbar.component.ts` — replace `color: white` (line 195) with `color: var(--color-text-on-primary)`, replace `#ec4899` (line 190 gradient) with `var(--color-accent-pink)` and add `var(--color-accent-pink-light)` for gradient endpoint
  - [x] 3.7 `styles.css` — replace `color: white` in `.skip-link` (lines 179, 185) with `color: var(--color-text-on-primary)`
  - [x] 3.8 `login.page.ts` — replace all `color: white` instances with `color: var(--color-text-on-primary)`
  - [x] 3.9 `change-password.page.ts` — replace all `color: white` instances with `color: var(--color-text-on-primary)`
  - [x] 3.10 `residents.page.ts` — replace `color: white` instances (lines 332, 338, 434) with `color: var(--color-text-on-primary)`, replace `#ec4899` with `var(--color-accent-pink)`
  - [x] 3.11 Feature pages (service-providers, administration, vehicles, apartments, condominiums, visits) — replace all `color: white` instances with `color: var(--color-text-on-primary)`
  - [x] 3.12 Run `grep -rn 'color: white' src/Web/ControlEasyReborn.Web/src/app/` and confirm zero results
  - [x] 3.13 Run `grep -rn '#ec4899' src/Web/ControlEasyReborn.Web/src/app/` and confirm zero results

- [x] 4. **Refactor login page to use design system components**
  - [x] 4.1 Replace `.ce-button` CSS class usage in template with `<ce-button>` component
  - [x] 4.2 Replace `.ce-input-group/wrapper/label/input/error` CSS with `<ce-input>` component usage
  - [x] 4.3 Replace `.ce-checkbox-box` CSS with `<ce-checkbox>` component usage
  - [x] 4.4 Replace `.ce-spinner` CSS with `<ce-spinner>` component usage
  - [x] 4.5 Remove all duplicated design system CSS from `login.page.ts` styles block
  - [x] 4.6 Add necessary component imports to `LoginPage`

- [x] 5. **Refactor change-password page to use design system components**
  - [x] 5.1 Replace duplicated `.ce-button`, `.ce-input-group`, `.ce-input-label`, `.ce-input`, `.ce-input-error`, `.ce-checkbox-box` CSS with component usage
  - [x] 5.2 Replace `.login-card`, `.login-brand`, `.login-brand-mark` CSS — evaluate if these should remain as page-specific styles or if card component can be reused
  - [x] 5.3 Remove all duplicated design system CSS from `change-password.page.ts` styles block
  - [x] 5.4 Add necessary component imports to `ChangePasswordPage`

- [x] 6. **Refactor dashboard page to use `<ce-card>` component**
  - [x] 6.1 Replace inline `.ce-card` and `.ce-card.accent-primary` CSS with `<ce-card accent="primary">` component usage
  - [x] 6.2 Remove duplicated card CSS from dashboard styles
  - [x] 6.3 Replace inline `var(--spacing-*)` with `var(--space-*)` in template (lines 14-15)

- [x] 7. **Fix modal component: remove dead CDK Overlay code**
  - [x] 7.1 Remove `Overlay`, `OverlayRef`, `OverlayModule`, `FocusTrap`, `FocusTrapFactory`, `A11yModule`, `NgTemplateOutlet`, `ElementRef`, `ViewChild` imports
  - [x] 7.2 Remove `overlay`, `overlayRef`, `modalTpl` properties
  - [x] 7.3 Remove `ngOnChanges`, `openModal`, `closeModal` methods
  - [x] 7.4 Add `(document:keydown.escape)` host event to call `close()`
  - [x] 7.5 Add focus management: on open, focus the modal dialog element; on close, restore focus to the trigger element

- [x] 8. **Fix dropdown component template positioning**
  - [x] 8.1 Restructure template so `.ce-dropdown-panel` is rendered inside `.ce-dropdown` wrapper (or make host element `position: relative` so panel positions correctly)
  - [x] 8.2 Remove unused `OverlayModule`, `Overlay`, `OverlayRef`, `NgTemplateOutlet`, `NgClass` imports
  - [x] 8.3 Verify dropdown positions correctly relative to its trigger

- [x] 9. **Fix tooltip directive**
  - [x] 9.1 Remove `prefers-reduced-motion: reduce` guard that prevents tooltips from showing entirely (line 27)
  - [x] 9.2 Add viewport edge clamping: check if tooltip would overflow right/bottom edges and adjust position
  - [x] 9.3 Remove unused `Overlay`, `FlexibleConnectedPositionStrategy`, `TemplateRef` imports

- [x] 10. **Fix tabs component to show/hide content**
  - [x] 10.1 Add `content` template input to `CeTabComponent`: `content = input.required<TemplateRef<unknown>>()`
  - [x] 10.2 Update `CeTabsComponent` template to conditionally render only the active tab's content using `ngTemplateOutlet`
  - [x] 10.3 Update `CeTabsComponent` to use `ContentChildren(CeTabComponent)` to query tabs
  - [x] 10.4 Verify tabs show/hide correctly when clicking different tab buttons

- [x] 11. **Fix breadcrumbs to use routerLink**
  - [x] 11.1 Add `RouterLink` import to `CeBreadcrumbsComponent`
  - [x] 11.2 Replace `[href]="crumb.route ?? '/'"` with `[routerLink]="crumb.route ?? '/'"`
  - [x] 11.3 Add `routerLinkActive` with `[routerLinkActiveOptions]="{exact: true}"` for aria-current support

- [x] 12. **Verify all changes**
  - [x] 12.1 Run `ng build` and confirm zero errors
  - [x] 12.2 Run `grep -rn 'var(--spacing-' src/Web/ControlEasyReborn.Web/src/app/` and confirm zero results (excluding `styles.css @theme` block)
  - [x] 12.3 Run `grep -rn 'color: white' src/Web/ControlEasyReborn.Web/src/app/` and confirm zero results
  - [x] 12.4 Run `grep -rn '#ec4899' src/Web/ControlEasyReborn.Web/src/app/` and confirm zero results
  - [ ] 12.5 Verify showcase page renders in light and dark themes
  - [ ] 12.6 Verify login page and change-password page use design system components
  - [ ] 12.7 Verify modal, dropdown, tabs, breadcrumbs functional behavior
  - [ ] 12.8 Verify dark theme toggle affects sidebar hover states

## Task Dependency Graph
```json
{
  "waves": [
    { "wave": 1, "tasks": ["1", "2", "3"] },
    { "wave": 2, "tasks": ["4", "5", "6", "7", "8", "9", "10", "11"] },
    { "wave": 3, "tasks": ["12"] }
  ]
}
```

**Note on Wave 2 parallelism:** Tasks 7-11 (component internal fixes) must NOT change component selectors or public input APIs. Tasks 4-6 (page refactoring) consume these components, so their external contracts must remain stable during Wave 2.

## Out of Scope (Follow-up Spec)

- Refactor all remaining feature pages (residents, visits, vehicles, apartments, service-providers, administration, condominiums) to use design system components instead of duplicating component CSS. Current tasks address `color: white` and `--spacing-*` in these pages but not the duplicated style patterns.
- Consider `--color-text-on-primary` dark mode value — some dark mode designs use `rgba(0,0,0,0.87)` for text on primary-colored backgrounds.
- Migrate tooltip to CDK Overlay for robust positioning.
- Migrate dropdown to CDK Overlay for robust positioning.
- Migrate modal to CDK Overlay for proper portal rendering.