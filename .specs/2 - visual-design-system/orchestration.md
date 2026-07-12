# Orchestration Log — Visual Design System

Coordination log for the **Visual Design System** spec (`.specs/2 - visual-design-system/`). Owned by the Frontend Agent.

## Task

Produce a complete spec (no code, no implementation) for the visual design system of the new ControlEasy Reborn Angular SPA, matching the **TCS Dashboard** reference (React + Vite + Tailwind CSS v4, served at `http://localhost:8080`).

## Classification

- **Type:** Feature spec (greenfield design system + showcase).
- **Agent:** Frontend Agent.
- **Companion specs:**
  - `.specs/1 - modernization-roadmap/` — sets the foundation, including the Angular app scaffold in task 1.8.
  - Future specs (e.g. `.specs/3 - dashboard/`, `.specs/4 - residents/`) will consume this design system.

## Decisions

1. **Reference-driven visual parity.** Every color, radius, shadow, and animation value comes from the TCS Dashboard's compiled CSS. No invented design language; the goal is to ship a system the team already trusts visually.
2. **Tailwind CSS v4 + CSS custom properties.** The design system is **tokens-first**: a set of `*.css` files under `src/app/design-system/tokens/` defines every primitive on `:root` and `[data-theme=dark]`. A Tailwind v4 `@theme` block exposes those variables as utility classes (`bg-primary`, `rounded-lg`, `shadow-md`). Deleting the Angular build pipeline still leaves a valid, documented system in plain CSS.
3. **Angular CDK for headless behavior.** `Modal`, `DropdownMenu`, and `Tooltip` use `@angular/cdk/overlay`, `cdkTrapFocus`, and `cdkMenu` for focus trapping, outside-click handling, and keyboard navigation. CDK is **not** used for visual styling.
4. **No Angular Material.** See "Open items" below — this is the open conflict with task 1.8.
5. **ThemeService owns the `<html data-theme>` attribute.** Three modes (`light`/`dark`/`system`), persisted in `localStorage["ce.theme"]`, SSR-safe. An inline FOUC-prevention script in `index.html` reads `localStorage` before any CSS applies.
6. **`--color-primary-raw` / `--color-primary` split.** Exposes a brand-customization hook for a future tenant re-skin without changing utility-class meanings.
7. **Showcase page is the contract.** `/design-system/showcase` renders one of every base component in every variant and both themes, with a Playwright visual-regression snapshot and an axe-core a11y scan. The showcase is what future feature pages are measured against.
8. **Accessibility is non-negotiable.** WCAG 2.1 AA, `:focus-visible` rings, ARIA roles, keyboard navigation, `prefers-reduced-motion` handling, and ≥ 44×44px touch targets are part of every component's acceptance criteria, not an add-on.
9. **Strict TypeScript + standalone components + signals.** Matches the project-level `AGENTS.md`. No NgModules, no `getComputedStyle` reads, no hex literals in component code.
10. **Mobile-first responsive.** Sidebar collapses to a drawer below `sm` and an icon rail between `sm` and `md`. Topbar condenses. Tables become stacked cards on `< sm` (unless `compact` density is requested).

## Open items — RESOLVED (2026-06-10)

- **CR-1 (Material vs Tailwind v4) — RESOLVED: option (a) approved.** `.specs/1 - modernization-roadmap/tasks.md` task 1.8 has been updated to read: *"...TypeScript strict mode, **Tailwind CSS v4 + custom design tokens**, **Angular CDK** for headless overlays..."* This spec proceeds.
- **i18n — IN SCOPE for v1.** `@angular/localize` will be added. Default locale **`pt-BR`** (Brazilian Portuguese, primary audience per the legacy system), with `en-US` as the secondary locale. User-facing strings on the showcase and on the layout shell get `data-i18n` hooks. Acceptance criteria for components include "no hard-coded user-facing strings in templates."
- **Icon library — APPROVED: Lucide (`lucide-angular`).** Tree-shakeable, line-style icons, matches the reference's iconography.
- **Generated OpenAPI types committed to git — APPROVED.** Continue with the project-level `AGENTS.md` decision; CI regenerates on each `ng-openapi-gen` run; types are checked in.
- **Showcase URL — APPROVED: `/design-system/showcase`.** Linked from the user menu's "Help" entry.

## Additions (2026-06-12)

- **Login screen — ADDED.** The spec was missing a login page, which is the first screen every user sees and the only page rendered outside `AppShell`. Added:
  - `requirements.md` UC-035 through UC-042: login form, error states, tenant picker, theme toggle on login, responsive login, accessibility, session-expired redirect, developer story.
  - `design.md` Login page section: route/guard, composition diagram, layout CSS, login flow sequence diagram, error states table, "Remember my email" behavior, password show/hide toggle, session-expired redirect, theme toggle on login, dark mode on login, accessibility (login-specific), and the new `ce-checkbox` base component.
  - `tasks.md` task D.7 (with subtasks D.7.1–D.7.9): LoginPageComponent, login form, error handling, tenant picker, theme toggle, session-expired redirect, responsive layout, AuthGuard, and Jasmine specs.
  - `tasks.md` task C.17 (renumbered from C.17 to add `ce-checkbox`): `ce-checkbox` base component.
  - `penpot-handoff.md`: added `checkbox.svg` to the component mapping, `login.svg` to the screen flow, updated deliverables count.
  - The login page integrates with the multi-tenant auth model defined in `.specs/1 - modernization-roadmap/design.md` (JWT claims, `GET /api/v1/security/tenants?email=...`, `POST /api/v1/auth/login`, `POST /api/v1/security/tenant-switch`).

## Penpot artifacts produced (2026-06-12)

- **`docs/penpot/svg/components/checkbox.svg`** — New base component SVG showing all states (unchecked, checked, indeterminate, disabled, focus) in light and dark themes. Maps to `ce-checkbox` (UC-042).
- **`docs/penpot/screens/login.svg`** — New screen mockup showing the login page in 3 responsive breakpoints (375px mobile, 768px tablet, 1440px desktop) in both light and dark themes, plus error states (401, 429), the tenant picker (multi-tenant), and the session-expired toast. Maps to UC-035 through UC-042.
- **`docs/penpot/manifest.json`** — Updated: added `ce-checkbox` component entry and `login` screen entry.
- **`docs/penpot/design-system.penpot.json`** — Updated: added `ce-checkbox` component definition and `screen-login` entry. Version remains `0.1.0` (no breaking change; these are additive).
- **`docs/penpot/tokens.json`** — Updated: added `ce-checkbox` to `metadata.componentSelectors`.

All open items resolved. Implementation proceeds in phases per `tasks.md` (A → F), starting with **Phase A — Project Setup** (Tailwind v4 + CDK + i18n bootstrap, no Material).

## Project Discovery confirmation

The Project Discovery one-time bootstrap was written into `/Users/felipecoelhosilva/Repos/ControlEasy/.kilo/agents/agents/FrontendAgent/AGENTS.md`:

- **Project Overview** filled in with the ControlEasy Reborn context, the Angular 18+ / signals / standalone plan, the JWT auth model, the DBTools_SQL backend dependency, the `ng-openapi-gen` code-gen flow, the Strangler Fig migration pattern, the primary user roles, and the current greenfield state.
- **Technical details** filled in with the Angular stack, the Tailwind v4 + CDK styling decision (and the open Material conflict), the planned folder layout (`design-system/{tokens,theme,components,showcase}`), the `ce-` selector-prefix and `OnPush` / strict-mode naming conventions, the chosen icon/charts/maps libraries, and the i18n "out of v1" stance.
- The **Project Discovery** section header was left in place (not removed) because the AGENTS.md section explicitly says to remove it only after the user opts not to answer the survey; since no survey was run (the user pre-supplied the answers in the task prompt), the section header remains as a record of the bootstrap process.
