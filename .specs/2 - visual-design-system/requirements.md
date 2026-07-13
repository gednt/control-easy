# Requirements — Visual Design System

> Companion to `design.md` and `tasks.md`. The visual design system for the new ControlEasy Reborn Angular SPA. Visual + behavioral parity with the **TCS Dashboard** reference (React + Vite + Tailwind CSS v4, served at `http://localhost:8080`).

## Scope

This spec covers **tokens, theming, base components, layout shell, login page, responsive behavior, accessibility, dark mode, brand customization hooks, and a Storybook-style preview page**. It does **not** cover feature pages (Residents, Visits, etc.) — those are owned by later specs. The login page is included because it is the first screen every user sees and it composes base components from this design system; its backend contract (`POST /api/v1/auth/login`, `POST /api/v1/auth/refresh`, `GET /api/v1/security/tenants?email=...`, `POST /api/v1/security/tenant-switch`) is defined in `.specs/1 - modernization-roadmap/design.md`.

## User stories

### Design tokens

- **UC-001:** As a frontend developer, I want a single source of truth for color, radius, spacing, typography, shadow, and motion tokens so that the visual language is consistent and trivially changeable.
  - **AC:** A `src/app/design-system/tokens/*.css` set defines CSS custom properties on `:root` and `[data-theme=dark]`. Token names match the reference (e.g. `--color-primary`, `--radius-lg`). Every base component consumes tokens, never hard-coded hex/px values.

- **UC-002:** As a designer, I want spacing, radii, type scale, and shadow scales exposed as named tokens so that I can adjust the system without touching component code.
  - **AC:** Tailwind v4 `@theme` block mirrors the CSS variables so utility classes (`rounded-lg`, `shadow-md`, `text-sm`) resolve to the same values as `var(--radius-lg)`, `var(--shadow-md)`, `var(--font-size-sm)`.

### Theming (light / dark)

- **UC-003:** As an end user, I want the app to follow my OS color preference on first load so that I do not have to configure anything.
  - **AC:** When `localStorage["ce.theme"]` is unset, the initial `data-theme` attribute on `<html>` matches `prefers-color-scheme: dark`. No FOUC (flash of unstyled content) on page load.

- **UC-004:** As an end user, I want to toggle between light, dark, and system modes from the topbar so that I can adapt to the room I am in.
  - **AC:** Theme toggle in the topbar cycles `light → dark → system → light`. The choice is persisted in `localStorage` under `ce.theme` and re-applied on every navigation/reload.

- **UC-005:** As an end user, I want my theme choice to be remembered across browser sessions so that I do not have to re-pick it on every visit.
  - **AC:** Re-opening the app after a full browser restart applies the same `data-theme` attribute that was last set.

### Layout shell

- **UC-006:** As an end user, I want a persistent left sidebar with the app's main navigation so that I can jump between modules with one click.
  - **AC:** A dark `w-64` (256px) sidebar with the brand mark at the top, a primary nav (Dashboard, Residents, Visits, Vehicles, Service Providers, Administration, Settings), and a user/tenant footer block. Active route is highlighted with `--sidebar-active-bg` / `--sidebar-active-text`. Sidebar collapses to an icon-only rail below `md` and becomes a slide-in drawer below `sm`.

- **UC-007:** As an end user, I want a sticky topbar with breadcrumbs, a global search field, a theme toggle, and a user menu so that the most common actions are always one click away.
  - **AC:** Topbar is `sticky top-0`, `h-16`, has a left slot for the mobile menu button + breadcrumbs, a center/right slot for global search, a theme-toggle button, a notifications bell (optional, out of v1 scope), and an avatar+dropdown user menu.

- **UC-008:** As an end user, I want the main content area to use the full remaining viewport width and to scroll independently from the sidebar so that long tables do not push the sidebar off-screen.
  - **AC:** `AppShell` uses CSS grid: `grid-cols-[16rem_1fr]` on `md+`, single column on mobile (sidebar becomes overlay drawer). Main area has its own `overflow-y-auto`.

### Base components

For each component, the acceptance criteria include: a prop table in `design.md`, an Angular standalone component in `src/app/design-system/components/`, a unit test (Jasmine), and a working entry in the showcase page.

- **UC-009:** **Button** — As a developer, I want primary, secondary, ghost, and danger variants in sm/md/lg sizes with loading and disabled states.
  - **AC:** `ce-button` accepts `variant: 'primary' | 'secondary' | 'ghost' | 'danger'`, `size: 'sm' | 'md' | 'lg'`, `loading: boolean`, `disabled: boolean`, `type: 'button' | 'submit' | 'reset'`. Hover on primary: `translateY(-1px)` + primary-tinted shadow. Disabled and loading render `aria-disabled` and `aria-busy` correctly.

- **UC-010:** **Card** — As a developer, I want a generic surface component with optional header, body, footer, and a left accent border for status semantics.
  - **AC:** `ce-card` accepts `[accent]: 'primary' | 'success' | 'warning' | 'danger' | 'info' | null` (renders a `border-l-4` in the matching token color), `header`, `default` (content projection for body), `footer` slots. Rounded `var(--radius-xl)`, soft `var(--shadow-card)`, `bg-surface-elevated`.

- **UC-011:** **Input** — As a developer, I want a styled text input with label, helper, error, prefix/suffix slot, and a clear focus ring.
  - **AC:** `ce-input` is a thin wrapper around `<input>` that projects `label`, `helper`, `error`, `prefix`, `suffix`. Focus ring is `0 0 0 3px` of `color-mix(in oklch, var(--color-primary) 15%, transparent)`. Error state swaps the ring to `--color-danger` and sets `aria-invalid="true"` and `aria-describedby` to the error message id.

- **UC-012:** **StatTile** — As a developer, I want a KPI tile with label, big number, trend chip, and sparkline-friendly content slot.
  - **AC:** `ce-stat-tile` accepts `label: string`, `value: string | number`, `[trend]: { direction: 'up' | 'down' | 'flat'; value: string; tone: 'success' | 'danger' | 'neutral' } | null`. Trend chip uses the matching token (`--color-success`, `--color-danger`). Used on the dashboard.

- **UC-013:** **StatusBadge** — As a developer, I want a small colored pill for status values.
  - **AC:** `ce-badge` accepts `tone: 'primary' | 'success' | 'warning' | 'danger' | 'info' | 'neutral'`, `size: 'sm' | 'md'`. Renders a `rounded-full` pill with the tone's `-light` background and the tone's strong color for the text/border. `aria-label` is the text.

- **UC-014:** **Modal** — As a developer, I want a focus-trapped dialog built on Angular CDK Overlay.
  - **AC:** `ce-modal` accepts `[open]`, `[title]`, `[size]`, projects `default` (body) and named slots `header`, `footer`. Uses CDK `cdkTrapFocus`, closes on `Escape` and on backdrop click, restores focus to the trigger element on close, renders `role="dialog"`, `aria-modal="true"`, `aria-labelledby` to the title id. Animates with `animate-zoom-in`.

- **UC-015:** **Toast** — As a developer, I want a non-blocking notification stack with auto-dismiss and the same tones as `StatusBadge`.
  - **AC:** A `ToastService` exposes `success/info/warning/danger(message, opts?)`. Toasts stack bottom-right, animate in (`toast-in`) and out (`toast-out`), auto-dismiss after `opts.duration ?? 5000ms`, can be dismissed by click, respect `prefers-reduced-motion`. Container uses `role="region" aria-live="polite"`.

- **UC-016:** **Table** — As a developer, I want a presentational table with sticky header, row hover, and a defined cell-padding rhythm.
  - **AC:** `ce-table` is a presentational wrapper around `<table>`; consumers project `<ng-template ceTh>` / `<ng-template ceTd>` for column templating. Sticky `<thead>`, hover `bg-surface/50`, border tokens, cell padding `--space-3 --space-4`. Sorting/pagination live in `ce-pagination` and are not part of the table component itself.

- **UC-017:** **EmptyState** — As a developer, I want a centered illustration + headline + description + optional CTA for "no data" screens.
  - **AC:** `ce-empty-state` accepts `icon` (Lucide name), `title`, `description`, `[actionLabel]`, `(action)`. Centered, vertical layout, `text-text-secondary` for the description.

- **UC-018:** **Spinner** — As a developer, I want a loading indicator that matches the theme.
  - **AC:** `ce-spinner` accepts `size: 'sm' | 'md' | 'lg'`, `tone: 'primary' | 'current'`. Uses the `spin-slow` keyframe; respects `prefers-reduced-motion` (becomes a static dashed circle).

- **UC-019:** **Avatar** — As a developer, I want a circular image-or-initials avatar for the user menu and resident lists.
  - **AC:** `ce-avatar` accepts `src`, `name`, `size`. Falls back to initials (first letter of up to two words) on a deterministic background chosen from the name's hash. Has `alt` set to the user's name.

- **UC-020:** **Tabs** — As a developer, I want accessible tabs (roving tabindex, arrow-key navigation).
  - **AC:** `ce-tabs` exposes a `tabs` input or projected `ce-tab` children. Uses the WAI-ARIA tabs pattern. Active indicator animates between tabs with a 150ms transition.

- **UC-021:** **DropdownMenu** — As a developer, I want an accessible menu triggered by a button, anchored via CDK Overlay.
  - **AC:** `ce-dropdown` projects a trigger (`[ceDropdownTrigger]`) and a `default` content. Opens on click, closes on `Escape`/outside click, navigates items with `↑/↓`, activates with `Enter`/`Space`. `role="menu"` / `role="menuitem"`.

- **UC-022:** **Tooltip** — As a developer, I want a hover/focus tooltip on icon-only buttons.
  - **AC:** `ce-tooltip` is a directive: `button[ceTooltip]="text"`. Uses CDK Overlay positioning. Shows after 400ms hover, hides on leave. `role="tooltip"`. Disabled when `prefers-reduced-motion`.

- **UC-023:** **Pagination** — As a developer, I want a pagination bar for tables.
  - **AC:** `ce-pagination` accepts `page`, `pageSize`, `total`, `(pageChange)`, `(pageSizeChange)`. Renders first/prev/next/last + a page list with ellipsis for large totals. `aria-label="Pagination"`.

- **UC-024:** **Breadcrumbs** — As a developer, I want a breadcrumb trail fed by the router.
  - **AC:** `ce-breadcrumbs` reads the current route's `data.breadcrumb` and parent route breadcrumbs, plus a static `crumbs` input override. Renders an `<nav aria-label="Breadcrumb">` with an `<ol>` and `aria-current="page"` on the last item.

### Login page

The login page is the entry point for every user role (`PlatformAdmin`, `TenantAdmin`, `Morador`, `AttendantProfile`). It is the only page in the app that renders **outside** the `AppShell` layout — no sidebar, no topbar. The login flow must handle the multi-tenant architecture defined in `.specs/1 - modernization-roadmap/design.md` (a single `User` may belong to multiple tenants; the login screen must resolve which tenant the user wants to operate in).

- **UC-035:** As an unauthenticated user, I want a login page with email and password fields so that I can authenticate and access the application.
  - **AC:** The route `/login` renders a `LoginPageComponent` that is **not** wrapped in `AppShell`. The page displays a centered `ce-card` on `var(--color-background)` containing: a brand mark (the ControlEasy logo), an email `ce-input`, a password `ce-input` with a show/hide toggle, a "Remember me" `ce-checkbox` (stores email in `localStorage["ce.email"]`), and a primary `ce-button` labeled "Sign in". All interactive targets are ≥ 44×44px. The form submits on `Enter` from either field.

- **UC-036:** As a user with invalid credentials, I want a clear error message so that I know what went wrong and how to fix it.
  - **AC:** On HTTP 401 or 400 from `POST /api/v1/auth/login`, the card shows a `ce-badge` with `tone="danger"` above the form reading "Invalid email or password" (or the server's localized `ProblemDetails.detail`). The email and password fields are not cleared. The error message is announced via `aria-live="assertive"` on the error region. Rate-limited responses (429) show "Too many attempts. Please try again in a few minutes." The password field gets `aria-invalid="true"` and the error message id is set as `aria-describedby`.

- **UC-037:** As a user who belongs to multiple tenants, I want to select which tenant to log in to so that I am scoped to the right condominium.
  - **AC:** After a successful `POST /api/v1/auth/login`, if the response includes more than one tenant in the `tenants` array, the login card transitions (fade, 200ms) to a **tenant picker** view showing a list of tenant cards (`ce-card` with `accent="primary"`) — each displaying `tenant.displayName` and the user's `userDisplayName` for that tenant. Tapping a tenant card calls `POST /api/v1/security/tenant-switch { tenantId }` (or uses the pre-selected tenant from the login response if only one). If only one tenant exists, the picker is skipped and the user proceeds directly to the dashboard. The picker is also reachable later from the topbar user menu ("Switch tenant").

- **UC-038:** As a user, I want the login page to follow my OS color preference and to let me toggle between light and dark mode on the login page itself, so that I am not stuck in the wrong theme before I can access the topbar.
  - **AC:** A theme toggle button (`sun`/`moon` Lucide icon) is placed in the top-right corner of the login page (outside the card, pinned to the viewport). It uses `ThemeService.setTheme()` to cycle `light → dark → system → light`. The FOUC-prevention script from UC-005 applies before the Angular bundle loads, so the login page never flashes the wrong theme.

- **UC-039:** As a user, I want the login page to be responsive so that I can log in on a phone, tablet, or desktop.
  - **AC:** Below `sm` (640px), the login card fills the viewport width with `mx-4` horizontal padding; above `sm`, the card is centered at `max-w-md` (28rem). The brand mark scales: `h-8` (32px) on mobile, `h-12` (48px) on `md+`. Input labels and buttons stack vertically with no wasted horizontal space below `sm`. The tenant picker grid switches from 1-column on `< sm` to 2-column on `md+`.

- **UC-040:** As a user, I want the login page to be accessible so that I can log in using only a keyboard or a screen reader.
  - **AC:** The page has a skip-to-content link that jumps to `<main id="login">`. The `<h1>` reads "Sign in to ControlEasy" (i18n key `LOGIN.HEADING`). The email `<label>` explicitly references the input via `for`/`id`. The password show/hide button has `aria-label="Show password"` / `aria-label="Hide password"`. The "Remember me" checkbox has `aria-label="Remember my email"`. Tab order: email → password → show/hide → remember me → sign-in button → theme toggle. After a failed login, focus returns to the email field. After a successful login + tenant pick, focus moves to `<main id="main">` in the `AppShell`.

- **UC-041:** As a user whose session has expired, I want to be redirected to the login page with a toast message so that I do not wonder why my screen went blank.
  - **AC:** When the `AuthService` detects a 401 response (expired/invalid JWT) and the refresh token is also expired/invalid, the `AuthInterceptor` redirects to `/login?reason=session-expired`. The login page reads the `reason` query param and, if present, shows a `ToastService.info("Your session has expired. Please sign in again.")` toast on mount. The toast respects `prefers-reduced-motion`. The email field is pre-filled from `localStorage["ce.email"]` if "Remember me" was previously checked.

- **UC-042:** As a developer, I want the login page to be a standalone Angular component that consumes only design-system base components and `AuthService` so that it is consistent with the rest of the app and trivially themeable.
  - **AC:** `LoginPageComponent` is a standalone component at `src/app/features/auth/login/login.page.ts`. It imports `CeButtonComponent`, `CeInputComponent`, `CeCardComponent`, `CeBadgeComponent`, `CeSpinnerComponent`, `CeCheckboxComponent` (new — a thin wrapper around native `<input type="checkbox">` with `ce-` styling), and `ThemeService`. It calls `AuthService.login(email, password)` which returns `Observable<LoginResponse>` with `{ accessToken, refreshToken, tenants }`. No hex literals, no inline styles, no Angular Material. The component is covered by a Jasmine spec that tests: render, form validation, error display, tenant picker flow, and redirect after login.

### Responsive behavior

- **UC-025:** As an end user on a phone, I want the app to be usable one-handed so that the gatehouse attendant can use it while standing.
  - **AC:** Mobile-first CSS. Below `sm` (640px), the sidebar becomes a slide-in drawer, the topbar condenses, the table becomes a stacked card list if `data-density="compact"` is not set, and all interactive targets are ≥ 44×44px.

- **UC-026:** As an end user on a tablet, I want the sidebar to be permanently visible (icon-only) so that I do not lose screen space.
  - **AC:** Between `sm` and `lg`, the sidebar is `w-16` and shows only icons + tooltips.

### Accessibility (WCAG 2.1 AA)

- **UC-027:** As a keyboard-only user, I want every interactive element to be focusable, in a logical tab order, and to have a visible focus ring.
  - **AC:** `:focus-visible` outline uses `outline: 2px solid var(--color-primary); outline-offset: 2px;` everywhere. Tab order follows DOM order. Skip-to-content link is the first focusable element on every page.

- **UC-028:** As a screen-reader user, I want all components to have correct ARIA semantics.
  - **AC:** Modal, dropdown, tabs, toast, pagination, breadcrumb, table, and button-loading components all expose the right roles, names, and live-region politeness. Verified by inspecting the accessibility tree in Chrome.

- **UC-029:** As a user with motion sensitivity, I want animations to be reduced when I have `prefers-reduced-motion: reduce` set.
  - **AC:** All keyframes (`slide-in`, `fade-in`, `zoom-in`, `toast-in`, `toast-out`, `pulse-subtle`, `spin-slow`) collapse to a `0ms` transition when the media query matches. Verified in Chrome DevTools "Emulate CSS media feature prefers-reduced-motion".

- **UC-030:** As a low-vision user, I want text and UI colors to meet WCAG 2.1 AA contrast.
  - **AC:** `--color-text-primary` on `--color-background` and on `--color-surface-elevated` ≥ 4.5:1. `--color-text-secondary` on the same surfaces ≥ 4.5:1. `--color-primary` (indigo `#4f46e5`) on white meets AA for ≥ 18px text. Verified with a contrast script before handoff.

### Dark mode toggle and persistence

- **UC-031:** As a developer, I want a `ThemeService` that owns the current theme and applies it to `<html data-theme="...">` so that all components stay in sync.
  - **AC:** `ThemeService` is `providedIn: 'root'`, exposes a `theme$: Signal<'light' | 'dark' | 'system'>` and a `setTheme(theme)` method. It writes `localStorage["ce.theme"]` and applies the resolved `data-theme` attribute. SSR-safe (guards `document` and `window`).

- **UC-032:** As a developer, I want components to react to the active theme via CSS only, never via JS reads of CSS variables, so that the framework stays simple.
  - **AC:** No `getComputedStyle` reads in components. The `[data-theme=dark]` selector on `<html>` re-styles everything via CSS custom properties.

### Brand customization

- **UC-033:** As a tenant admin, I want a future feature flag that allows re-coloring the primary token to the tenant's brand color.
  - **AC:** `--color-primary` is exposed as `--color-primary-raw` (the same value) plus `--color-primary` (which is `var(--color-primary-raw)` by default). A future "tenant brand" service can override `--color-primary-raw` on the root of a `<div data-tenant="acme">` block. **This spec ships the hook but no admin UI for it.**

### Showcase page

- **UC-034:** As a designer or developer, I want a `/showcase` page that renders one of every base component, in every variant, in both themes, side-by-side so that I can review the system at a glance.
  - **AC:** A standalone routed page at `/design-system/showcase` (linked from the user menu under "Help → Design system"). Shows: every button variant/size, every card accent, every input state, every status badge tone, a sample modal (triggerable), a sample toast (triggerable), a sample table, every empty state, the spinner in every size, an avatar, tabs, a dropdown, a tooltip, pagination, breadcrumbs, and a theme-toggle demo. Theme can be flipped on the page itself for side-by-side comparison.
