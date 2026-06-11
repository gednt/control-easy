# Tasks — Visual Design System

> Companion to `requirements.md` and `design.md`. Phased, verifiable work. **Phases must be completed in order**; a phase gate (last task of the phase) must be green before starting the next phase.

> ## Conflict resolution (top-level)
>
> - [x] **CR-1** Update `.specs/1 - modernization-roadmap/tasks.md` task 1.8 to specify **Tailwind CSS v4 + custom design tokens, Angular CDK (headless only)** instead of Angular Material. Drop `@angular/material` from the planned `package.json`; add `tailwindcss@^4`, `@angular/cdk`, `lucide-angular`. **Resolved 2026-06-10 (option a approved).** Task 1.8 in `.specs/1 - modernization-roadmap/tasks.md` has been updated. Proceed with implementation.
> - [x] **i18n — IN SCOPE for v1.** Add `@angular/localize`, default locale `pt-BR`, secondary `en-US`. User-facing strings on showcase and shell get `data-i18n` hooks. Phase A task **A.7** added below.
> - [x] **Icon library — Lucide (`lucide-angular`).**
> - [x] **OpenAPI types committed to git** (inherited from `AGENTS.md`).
> - [x] **Showcase URL — `/design-system/showcase`.**

---

## Phase A — Setup

*Goal: empty Angular project compiles, Tailwind v4 is wired, the design-system folder exists.*

- [ ] **A.1** Scaffold the Angular 18+ workspace at `src/Web/ControlEasyReborn.Web` with `ng new --standalone --strict --style=css --routing=true --ssr=false` (if not already scaffolded in task 1.8). Add `angular.json` and `package.json` to git.
  - **Acceptance criteria:** `ng build` produces a `dist/` with no errors. `tsconfig.json` has `strict: true`, `noUncheckedIndexedAccess: true`, `exactOptionalPropertyTypes: true`.

- [ ] **A.2** Install runtime dependencies: `tailwindcss@^4`, `@tailwindcss/postcss`, `postcss`, `autoprefixer`, `@angular/cdk`, `lucide-angular`, `clsx`. **Do not install `@angular/material`** (see CR-1).
  - **Acceptance criteria:** `package.json` lists exactly these; `npm install` succeeds.

- [ ] **A.3** Configure PostCSS for Tailwind v4. Create `.postcssrc.json` with `{ "plugins": { "@tailwindcss/postcss": {} } }`. Create `tailwind.config.ts` with the content glob `./src/**/*.{html,ts}`.
  - **Acceptance criteria:** A one-line `styles.css` with `@import "tailwindcss";` and a `<div class="bg-red-500">x</div>` builds and renders red.

- [ ] **A.4** Create the `src/app/design-system/` folder tree (tokens, theme, components, showcase) per `design.md` folder layout.
  - **Acceptance criteria:** Empty `*.css` placeholders exist for each token file; `theme.service.ts` is a stub returning `theme: signal('system')`.

- [ ] **A.5** Add ESLint (`@angular-eslint`) and Prettier configs; add a `lint` and `format` script to `package.json`.
  - **Acceptance criteria:** `npm run lint` exits 0 on the empty project.

- [ ] **A.6** Add `proxy.conf.json` mapping `/api` → `https://localhost/api` so dev calls reach the API container (consumed by `ng serve`).
  - **Acceptance criteria:** `ng serve --proxy-config proxy.conf.json` starts; the proxy log line appears.

- [ ] **A.7** Add i18n: install `@angular/localize`, generate `xliff` extraction config (`i18n.extract`), wire `app.config.ts` with `provideRouter` + `withComponentInputBinding()` only (no global `$localize` registration yet — done in Phase B), set `angular.json` `i18n` block with `sourceLocale = "pt-BR"` and `locales = { "en-US": "src/locale/messages.en-US.xlf" }`. Default locale: `pt-BR` (Brazilian Portuguese, primary audience per the legacy system).
  - **Acceptance criteria:** `ng extract-i18n` produces an empty `messages.xlf` (no strings yet); build with `ng build --localize` succeeds; `localize` plugin is registered in `angular.json`.

- **Verification gate (Phase A):** `ng build` succeeds; `ng serve` shows the empty app at `http://localhost:4200`; one Tailwind utility class renders correctly; lint passes; `ng extract-i18n` runs.

---

## Phase B — Tokens + theme

*Goal: every token file exists, the Tailwind `@theme` block exposes them as utilities, the `ThemeService` works, no FOUC.*

- [ ] **B.1** Author `src/app/design-system/tokens/colors.css` with the full light + dark palettes from `design.md` (`--color-*` and `--sidebar-*`).
  - **Acceptance criteria:** All 19 light tokens + 18 dark tokens (plus 4 sidebar tokens) exist; the file is ~80 lines.

- [ ] **B.2** Author `radii.css`, `spacing.css`, `type.css`, `shadow.css`, `motion.css` with the tables from `design.md`.
  - **Acceptance criteria:** All named tokens (`--radius-sm`…`--radius-full`, `--space-1`…`--space-16`, `--font-size-*`, `--shadow-*`, `--duration-*`, `--ease-*`) exist.

- [ ] **B.3** Add `@import` lines for each token file at the top of `src/styles.css`, and the `@theme` block that maps variables to Tailwind utilities (`--color-primary: var(--color-primary)`, `--radius-lg: var(--radius-lg)`, etc.).
  - **Acceptance criteria:** In a scratch component, `class="bg-primary text-white rounded-lg shadow-md"` renders the indigo color, 12px radius, and the card shadow in both themes.

- [ ] **B.4** Implement `ThemeService` (`providedIn: 'root'`) with:
  - `theme: Signal<'light' | 'dark' | 'system'>`,
  - `resolvedTheme: Signal<'light' | 'dark'>`,
  - `setTheme(t)`, `toggle()`,
  - SSR-safe (`isPlatformBrowser` guard),
  - `localStorage["ce.theme"]` round-trip,
  - resolves `system` via `matchMedia('(prefers-color-scheme: dark)')`,
  - listens to `matchMedia.change` to re-resolve when in `system` mode,
  - writes `data-theme` attribute on `<html>`.
  - **Acceptance criteria:** A unit test asserts all six behaviors (initial read, set+persist, system resolution, `matchMedia` listener, SSR no-op, attribute write).

- [ ] **B.5** Apply `data-theme` on the server-rendered HTML to prevent FOUC. In `index.html` add an inline `<script>` (under 1KB) that reads `localStorage["ce.theme"]` and sets `document.documentElement.dataset.theme` before any CSS is applied.
  - **Acceptance criteria:** Hard-reload in Chrome with `localStorage["ce.theme"]="dark"` shows the dark theme on first paint (verified by screenshot).

- [ ] **B.6** Add the `prefers-reduced-motion` global rule in `styles.css` that collapses all keyframes/transitions to 0ms.
  - **Acceptance criteria:** Emulating `prefers-reduced-motion: reduce` in DevTools removes the `slide-in`/`zoom-in`/`spin-slow` animations.

- [ ] **B.7** Add the global `:focus-visible` outline rule in `styles.css`.
  - **Acceptance criteria:** Tabbing through any element shows a 2px indigo outline with 2px offset.

- [ ] **B.8** Import the named keyframes (`slide-in`, `fade-in`, `zoom-in`, `toast-in`, `toast-out`, `pulse-subtle`, `spin-slow`) in `styles.css` so Tailwind v4's `animate-*` utilities resolve.
  - **Acceptance criteria:** `<div class="animate-zoom-in">` in a scratch component runs the modal entrance animation.

- **Verification gate (Phase B):** Token files are present and documented; the showcase scratch page (Phase E) renders correctly in both themes with no FOUC; a unit-test run of `ThemeService` is green; a Playwright screenshot of a dark page matches the reference.

---

## Phase C — Base components

*Goal: every base component exists, has unit tests, and consumes only tokens.*

For each task, "exists" means: a standalone Angular component in `src/app/design-system/components/<name>/`, a `*.spec.ts` next to it, and a `*.stories.ts` consumed by the showcase page in Phase E.

- [ ] **C.1** **`ce-button`** — `variant`, `size`, `loading`, `disabled`, `type` inputs; `(click)` output; `aria-busy`, `aria-disabled`; hover `translateY(-1px)` + primary glow shadow for primary; respects `prefers-reduced-motion`.
  - **Acceptance criteria:** Spec covers all four variants × three sizes × loading × disabled; computed class string is asserted.

- [ ] **C.2** **`ce-card`** — `accent`, `padded` inputs; `[card-header]`, `[card-footer]`, `default` slots; `rounded-xl`, `--shadow-card`, optional `border-l-4` accent.
  - **Acceptance criteria:** Spec covers each accent tone and the `padded=false` flat variant.

- [ ] **C.3** **`ce-input`** — `label`, `helper`, `error`, `placeholder`; `ControlValueAccessor` for reactive forms; `[input-prefix]`, `[input-suffix]` slots; 3px focus ring at 15% primary alpha; error state swaps to danger ring + `aria-invalid="true"`.
  - **Acceptance criteria:** Spec covers a reactive-forms round-trip, error state, and ARIA attributes.

- [ ] **C.4** **`ce-stat-tile`** — `label`, `value`, optional `trend`; trend chip uses the right tone.
  - **Acceptance criteria:** Spec covers null trend, up-success, down-danger, flat-neutral.

- [ ] **C.5** **`ce-badge`** — `tone`, `size`; rounded-full pill with `-light` background; `aria-label` is the text.
  - **Acceptance criteria:** Spec covers every tone × size.

- [ ] **C.6** **`ce-modal`** — built on `@angular/cdk/overlay` + `cdkTrapFocus`; `open` two-way, `title`, `size`; closes on `Escape` and backdrop click; restores focus to trigger; `role="dialog"`, `aria-modal`, `aria-labelledby`; `animate-zoom-in` entrance.
  - **Acceptance criteria:** Spec covers open/close round-trip, focus trap (`Tab` stays inside), `Escape` closes, backdrop click closes, focus restoration.

- [ ] **C.7** **`ToastService` + `<ce-toast-host>`** — service with `success/info/warning/error(message, opts?)`; host component renders the stack at bottom-right; `aria-live="polite"`; auto-dismiss after duration; respects `prefers-reduced-motion`.
  - **Acceptance criteria:** Spec covers queue order, auto-dismiss timer, manual dismiss, and `prefers-reduced-motion` no-op.

- [ ] **C.8** **`ce-table`** — presentational wrapper; sticky `<thead>`; row hover `bg-surface/50`; `[ceTh]` / `[ceTd]` projected templates.
  - **Acceptance criteria:** Spec asserts sticky-header class, hover class, and that projected cell templates render.

- [ ] **C.9** **`ce-empty-state`** — `icon` (Lucide), `title`, `description`, `actionLabel`, `(action)`; centered vertical layout.
  - **Acceptance criteria:** Spec covers with/without CTA and icon rendering.

- [ ] **C.10** **`ce-spinner`** — `size`, `tone`; `spin-slow` keyframe; `prefers-reduced-motion` becomes a static dashed circle; `role="status" aria-label="Loading"`.
  - **Acceptance criteria:** Spec covers size, tone, and reduced-motion path.

- [ ] **C.11** **`ce-avatar`** — `src`, `name`, `size`; initials fallback on a deterministic background hashed from the name.
  - **Acceptance criteria:** Spec covers image path, initials path, and that the same name always gets the same background.

- [ ] **C.12** **`ce-tabs`** — projected `ce-tab` children; `role="tablist" / "tab" / "tabpanel"`; roving tabindex; `←/→` keys move + activate; animated underline.
  - **Acceptance criteria:** Spec covers keyboard navigation and ARIA wiring.

- [ ] **C.13** **`ce-dropdown`** — projected trigger (`[ceDropdownTrigger]`) + `default` slot; opens on click, closes on `Escape`/outside click; `↑/↓` navigate, `Enter`/`Space` activate; `role="menu" / "menuitem"`; CDK Overlay positioning.
  - **Acceptance criteria:** Spec covers open/close, keyboard nav, and outside-click close.

- [ ] **C.14** **`[ceTooltip]` directive** — show after 400ms hover, hide on leave; CDK Overlay positioning; `role="tooltip"`; disabled when `prefers-reduced-motion`.
  - **Acceptance criteria:** Spec covers show/hide timing and reduced-motion skip.

- [ ] **C.15** **`ce-pagination`** — `page`, `pageSize`, `total`, `pageSizeOptions`; first/prev/next/last + page list with ellipsis for large totals; `aria-label="Pagination"`.
  - **Acceptance criteria:** Spec covers edge cases (total=0, total=1, total=1000 with ellipsis), and `(pageChange)`/`(pageSizeChange)` emissions.

- [ ] **C.16** **`ce-breadcrumbs`** — reads `data.breadcrumb` from each activated route; `<nav aria-label="Breadcrumb">`, `<ol>`, `aria-current="page"` on the last item.
  - **Acceptance criteria:** Spec covers a three-level route tree.

- [ ] **C.17** Add a barrel `src/app/design-system/index.ts` re-exporting every component so feature pages can do `import { CeButtonComponent } from '@app/design-system';` (path alias configured in `tsconfig.json`).
  - **Acceptance criteria:** Importing from the barrel works from a feature page in Phase 3 of the modernization roadmap.

- **Verification gate (Phase C):** `ng test` (or `npm test`) is green for all 16 component specs; the showcase page (Phase E) renders every component; no component references a hex literal or a px value (verified by `git grep -E "#[0-9a-fA-F]{3,8}" src/app/design-system/components/` returning no results).

---

## Phase D — Layout shell

*Goal: every page lives inside `AppShell`, with the sidebar, topbar, and breadcrumbs.*

- [ ] **D.1** Implement `AppShellComponent` — CSS grid layout from `design.md`; renders `<ce-sidebar>`, `<ce-topbar>`, `<main id="main" tabindex="-1">` outlet, and a skip-to-content link.
  - **Acceptance criteria:** Skip link is the first focusable element and jumps to `<main>`.

- [ ] **D.2** Implement `SidebarComponent` — brand mark, primary nav (Dashboard, Residents, Visits, Vehicles, Service Providers, Administration, Settings), user/tenant footer. Active route highlight via router `routerLinkActive`. `w-64` on `md+`, `w-16` between `sm` and `md`, drawer on `< sm`.
  - **Acceptance criteria:** Active-route highlight uses the sidebar-active tokens; permissions filtering is stubbed (no behavior yet).

- [ ] **D.3** Implement the mobile drawer — CDK Overlay or simple `transform` transition (`slide-in`); closes on route navigation; `Escape` closes; backdrop is clickable.
  - **Acceptance criteria:** Resize to `< sm` shows the drawer trigger; clicking it slides the drawer in; selecting a nav item closes it.

- [ ] **D.4** Implement `TopbarComponent` — sticky, `h-16`, mobile-menu button, `<ce-breadcrumbs>`, global search input (collapses to icon on `< sm`), theme toggle, user-menu `<ce-avatar>` + `<ce-dropdown>`.
  - **Acceptance criteria:** Theme toggle cycles `light → dark → system → light` and the change is visible immediately.

- [ ] **D.5** Wire the `AppShell` as the root layout for every lazy feature route. Update `app.routes.ts` so every feature loads as `{ path: '', component: AppShellComponent, children: [...] }`.
  - **Acceptance criteria:** Navigating to `/residents`, `/visits`, etc., shows the shell; navigating to `/` shows a placeholder dashboard inside the shell.

- [ ] **D.6** Add a placeholder `DashboardComponent` (the showcase's "stats grid" + a "recent activity" card) so the shell is exercised end-to-end.
  - **Acceptance criteria:** `/` shows a `ce-stat-tile` × 4 grid and a `ce-card` with a "recent activity" list.

- **Verification gate (Phase D):** Every route renders inside the shell; the responsive breakpoints work in Chrome DevTools (375 / 768 / 1280); the theme toggle persists across reload; no console errors; the skip-link works in a Playwright keyboard test.

---

## Phase E — Preview / showcase page

*Goal: a single page that renders one of every base component in every variant and both themes.*

- [ ] **E.1** Create `src/app/design-system/showcase/showcase.page.ts` as a standalone component routed at `/design-system/showcase`. Add a "Help → Design system" entry in the user-menu dropdown that links to it.
  - **Acceptance criteria:** Route loads; the page renders; the menu entry navigates to it.

- [ ] **E.2** Build the page in named sections, one per component. Each section has a heading, a one-line description, and a live example with a code-snippet `<pre>` block (copy button is nice-to-have, not required).
  - **Sections:** Buttons, Cards, Inputs, StatTiles, Badges, Modal (triggerable), Toast (triggerable buttons per tone), Table (one example row), EmptyState, Spinner, Avatar, Tabs, Dropdown (triggerable), Tooltip (hover the example button), Pagination, Breadcrumbs, Theme toggle demo.
  - **Acceptance criteria:** Every component is visible; triggering the modal/toast/dropdown actually opens it.

- [ ] **E.3** Add a "Theme" switcher at the top of the showcase page that flips between `light` and `dark` *for that page only* (so a designer can compare side-by-side). The switch is independent of the global `ThemeService` and uses a local `[data-theme]` attribute on a wrapper div.
  - **Acceptance criteria:** Flipping the local switch re-skins the showcase without affecting the topbar/global theme.

- [ ] **E.4** Add a Playwright visual-regression script (`tests/visual/showcase.spec.ts`) that captures the showcase page at `375×812`, `768×1024`, and `1440×900` in both `light` and `dark`, and diffs against `tests/visual/__snapshots__/showcase-*.png`. Any pixel diff > 0.1% fails the build.
  - **Acceptance criteria:** Snapshots are committed; running the test on the unchanged page is green.

- [ ] **E.5** Add an a11y scan (`tests/a11y/showcase.spec.ts`) using `@axe-core/playwright`; any `serious` or `critical` violation fails the build.
  - **Acceptance criteria:** Scan runs against `/design-system/showcase`; the report is printed on failure; the first commit is green.

- **Verification gate (Phase E):** Visual-regression snapshots are committed and green; a11y scan is green; a designer can hit the URL and see one of every component in both themes.

---

## Phase F — Docs + handoff

*Goal: future developers (and agents) can find and use the system without spelunking.*

- [ ] **F.1** Add `docs/design-system/README.md` linking to `requirements.md`, `design.md`, this `tasks.md`, and the showcase URL. Include a "How to add a new base component" checklist.
  - **Acceptance criteria:** README exists, is reachable from the repo root `README.md`, and the checklist is accurate against the final code.

- [ ] **F.2** Add a "Brand customization" mini-how-to: explain the `--color-primary-raw` hook and how a future `TenantBrandService` will override it.
  - **Acceptance criteria:** The doc is one screen long and references the exact CSS line.

- [ ] **F.3** Add a "Theming guide" mini-how-to: explain the `ThemeService`, the FOUC-prevention script, and how to write a token-respecting component.
  - **Acceptance criteria:** The doc is one screen long and includes a "do/don't" code snippet.

- [ ] **F.4** Add the showcase page URL to the user-menu's "Help" entry and to the topbar's "?" icon (if any).
  - **Acceptance criteria:** Clicking the help entry navigates to `/design-system/showcase` in both light and dark.

- [ ] **F.5** Add a CHANGELOG entry under "Unreleased" with the design system phase, the Tailwind v4 decision (referencing CR-1), and a link to the showcase.
  - **Acceptance criteria:** `CHANGELOG.md` exists; the entry is dated.

- **Verification gate (Phase F):** A new developer (or a fresh agent) can read `docs/design-system/README.md` and the showcase page, and add a new feature page that uses only base components and tokens — without opening any other spec.

---

## Global verification (final)

- [ ] **G.1** `ng build --configuration production` succeeds.
- [ ] **G.2** `ng test` is green.
- [ ] **G.3** `npm run lint` is green.
- [ ] **G.4** Visual-regression snapshots on `/design-system/showcase` are green at three viewport sizes × two themes.
- [ ] **G.5** axe-core scan on `/design-system/showcase` reports zero `serious`/`critical` violations.
- [ ] **G.6** `git grep -E "#[0-9a-fA-F]{3,8}" src/app/design-system/components/` returns no results.
- [ ] **G.7** `git grep -E "@angular/material" src/Web/` returns no results (assuming CR-1 is approved).
- [ ] **G.8** Smoke-tested in Chrome via Playwright: page loads in both themes, theme toggle persists across reload, no FOUC, sidebar drawer opens on mobile width.
