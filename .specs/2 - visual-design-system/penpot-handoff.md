# Penpot hand-off - ControlEasy Reborn Visual Design System

> Status: produced 2026-06-11. Format: Penpot-importable JSON + a parallel SVG asset library. The two formats cover the same design surface and stay in sync via `docs/penpot/manifest.json`.

## Overview

This hand-off ships the source-of-truth design artifacts for the **Visual Design System** spec at `.specs/2 - visual-design-system/`. Every component, token, layout, and screen that the Angular SPA will implement has a counterpart here. The artifacts are intentionally **buildable, diffable, and reviewer-friendly** so the FrontendAgent (or any future designer) can pick them up, edit them in Penpot, and produce updated SVGs / JSON without breaking the contract with the Angular `styles.css` `@theme` block.

**Format chosen.** Penpot's native file format is a binary ZIP with a custom internal layout that is awkward to author by hand. A true `.penpot` file is not feasible from a CLI agent. Instead, this hand-off ships two complementary artifacts:

1. **`docs/penpot/design-system.penpot.json`** - a Penpot-importable JSON file. The `pages` block mirrors Penpot's four top-level pages (Tokens / Components / Layout Shell / Theme Variants). The `colors` and `typographies` blocks become shared library assets. The `components` block enumerates all 16 `ce-*` components with their variants, sizes, states, a11y annotations, and token references. A designer opens this file in Penpot via *Libraries > Import library* and uses it as the source of truth.
2. **`docs/penpot/svg/`** - a parallel SVG asset library (one SVG per component, one per layout, one per screen). The SVGs are pixel-readable reference deliverables: they show every state, size, and theme variant on a single canvas. They are the format a designer can drop into a Penpot page as drag-and-drop building blocks if they prefer to assemble the library visually rather than import the JSON.

Both formats are committed to the repo so PR reviewers can see design changes diff-by-diff. The `manifest.json` enumerates every file and binds each one to a `ce-*` selector and a user-story (`UC-XXX`) from `requirements.md`.

## Deliverables

| Path | Purpose |
|---|---|
| `docs/penpot/design-system.penpot.json` | Penpot-importable design system: pages, colors, typographies, components, layouts, screens. |
| `docs/penpot/tokens.json` | Machine-readable tokens (light + dark). The contract that gets diffed against the Angular `@theme` block. |
| `docs/penpot/manifest.json` | Index of every SVG file with its `ce-*` selector and `UC-XXX` binding. |
| `docs/penpot/svg/components/*.svg` | 16 component reference SVGs (one per `ce-*` selector). |
| `docs/penpot/svg/layout/app-shell.svg` | Sidebar + topbar + grid; mobile / tablet / desktop breakpoints annotated. |
| `docs/penpot/screens/*.svg` | 4 user-journey mockups: attendant dashboard, resident pre-register, tenant-admin attendant, platform-admin tenant. |

## Token name -> CSS variable mapping

The token names in `docs/penpot/tokens.json` are the canonical CSS custom property names defined in `.specs/2 - visual-design-system/design.md`. The Angular `styles.css` `@theme` block (Phase B of `tasks.md`) will reference exactly these names. Examples:

- `tokens.json["light"]["--color-primary"]` = `"var(--color-primary-raw)"` = the resolved light value `#4f46e5`.
- `tokens.json["light"]["--color-surface"]` = `"#ffffff"`.
- `tokens.json["light"]["--radius-lg"]` = `"12px"`.
- `tokens.json["light"]["--shadow-card"]` = `"0 1px 3px 0 rgb(0 0 0 / 0.08), 0 1px 2px -1px rgb(0 0 0 / 0.05)"`.
- `tokens.json["light"]["--duration-base"]` = `"200ms"`.

Dark-theme overrides live in the `tokens.json["dark"]` block. The only tokens that change between light and dark are: `--color-background`, `--color-surface`, `--color-surface-elevated`, `--color-border`, `--color-text-primary`, `--color-text-secondary`, `--color-primary-raw`, `--color-primary-hover`, `--color-primary-light`, `--color-danger`, `--color-danger-light`, `--color-warning`, `--color-warning-light`, `--color-success`, `--color-success-light`, `--color-info`, `--color-info-light`, and the shadow tokens. Spacing, radii, type, and motion tokens are theme-agnostic and identical in both blocks. The sidebar palette is also theme-agnostic (dark in both themes, per `design.md`).

The brand-customization hook is the split between `--color-primary-raw` (the value a tenant can override) and `--color-primary` (which is `var(--color-primary-raw)`). Both names are present in `tokens.json`. Per `UC-033`, a future `TenantBrandService` will write `data-tenant="acme"` on `<app-root>` and override `--color-primary-raw` at that scope.

## Component name -> Angular selector mapping

| Penpot component | Angular selector | Spec UC | SVG fallback |
|---|---|---|---|
| Button | `ce-button` | UC-009 | `docs/penpot/svg/components/button.svg` |
| Card | `ce-card` | UC-010 | `docs/penpot/svg/components/card.svg` |
| Input | `ce-input` | UC-011 | `docs/penpot/svg/components/input.svg` |
| StatTile | `ce-stat-tile` | UC-012 | `docs/penpot/svg/components/stat-tile.svg` |
| StatusBadge | `ce-badge` | UC-013 | `docs/penpot/svg/components/badge.svg` |
| Modal | `ce-modal` | UC-014 | `docs/penpot/svg/components/modal.svg` |
| Toast (service + host) | `ce-toast` | UC-015 | `docs/penpot/svg/components/toast.svg` |
| Table | `ce-table` | UC-016 | `docs/penpot/svg/components/table.svg` |
| EmptyState | `ce-empty-state` | UC-017 | `docs/penpot/svg/components/empty-state.svg` |
| Spinner | `ce-spinner` | UC-018 | `docs/penpot/svg/components/spinner.svg` |
| Avatar | `ce-avatar` | UC-019 | `docs/penpot/svg/components/avatar.svg` |
| Tabs | `ce-tabs` | UC-020 | `docs/penpot/svg/components/tabs.svg` |
| DropdownMenu | `ce-dropdown` | UC-021 | `docs/penpot/svg/components/dropdown.svg` |
| Tooltip (directive) | `[ceTooltip]` | UC-022 | `docs/penpot/svg/components/tooltip.svg` |
| Pagination | `ce-pagination` | UC-023 | `docs/penpot/svg/components/pagination.svg` |
| Breadcrumbs | `ce-breadcrumbs` | UC-024 | `docs/penpot/svg/components/breadcrumbs.svg` |

The layout shell is composed of three sub-components: `ce-sidebar` (with `data-breakpoint` semantics of expanded / collapsed / drawer), `ce-topbar`, and the root `AppShell` (CSS grid). See `docs/penpot/svg/layout/app-shell.svg` for the three annotated breakpoints.

## Screen flow

The four user-journey mockups in `docs/penpot/screens/` exercise the component library end-to-end and bind each one to a primary user role per `.specs/1 - modernization-roadmap/requirements.md` UC-9:

1. **`attendant-dashboard.svg`** - `AttendantProfile` (Attendant). The gatehouse operator sees the dashboard and opens the "Open new visit" modal after a phone confirmation.
2. **`resident-pre-register.svg`** - `Morador` (Resident). Maria Silva (apt 102-A) pre-registers her father for a Sunday afternoon visit. Tabs (Upcoming / Past / Recurring / Service providers) and a form card.
3. **`tenant-admin-attendant.svg`** - `TenantAdmin`. Renata Ferreira (Acme) opens the "New attendant profile" modal over the attendants list. Binds user + shift + gatehouse + permission set.
4. **`platform-admin-tenant.svg`** - `PlatformAdmin`. The platform operator creates a new tenant (Globex) with a brand color wired to `--color-primary-raw`.

Every screen is composed exclusively of design-system components - no one-off shapes. The annotation at the bottom of each SVG lists the components used.

## How to import into Penpot (step by step)

1. Open Penpot (cloud or self-hosted, any version that supports libraries - v2 format).
2. In the left sidebar, click **Libraries** (the stack-of-books icon).
3. Click the **Import library** button in the top-right of the Libraries panel.
4. Choose **`docs/penpot/design-system.penpot.json`** from this repo.
5. Penpot will create four pages in the current file (or a new file - confirm): **01 Tokens**, **02 Components**, **03 Layout Shell**, **04 Theme Variants**.
6. The shared library will register all 38 colors and 8 typographies under the `ce/...` prefix.
7. To re-create a specific component as a Penpot master component: open the relevant page, drag the `svgFallback` file from `docs/penpot/svg/components/` into the page, and use *Create component* on the resulting shape.
8. To re-create a screen: drag the SVG from `docs/penpot/screens/` onto a new page and replace each shape with the corresponding Penpot master component.

## How to verify visual parity with the Angular build

The Angular `styles.css` `@theme` block (Phase B, task B.3 of `.specs/2 - visual-design-system/tasks.md`) must reference every token in `docs/penpot/tokens.json`. A simple parity check is:

```bash
# 1. Extract every --token: value from the @theme block
grep -oE -- '--[a-z-]+' src/Web/ControlEasyReborn.Web/src/styles.css | sort -u > /tmp/css-tokens.txt

# 2. Extract every key from tokens.json (light + dark)
jq -r '.light, .dark | keys[]' docs/penpot/tokens.json | sort -u > /tmp/json-tokens.txt

# 3. Diff
diff /tmp/css-tokens.txt /tmp/json-tokens.txt
```

The diff should be empty. If a token is added to `tokens.json` but missing from `styles.css`, the design system has drifted and the Angular build needs a token file update (Phase B). If a token is in `styles.css` but missing from `tokens.json`, the design system is the source of truth and `tokens.json` needs a new entry.

A second, faster check is a visual regression against the **showcase page** at `/design-system/showcase` once Phase E lands (Playwright captures at 375x812, 768x1024, and 1440x900, in both `light` and `dark`; compare against committed snapshots in `tests/visual/__snapshots__/showcase-*.png`).

A third, accessibility-level check is `tests/a11y/showcase.spec.ts` (axe-core) - any `serious` or `critical` violation blocks the build. The a11y annotations in every component SVG (focus ring, ARIA role, keyboard pattern) are the visual reference for what axe-core will inspect at runtime.

## Versioning policy

The design system follows **semantic versioning** of the format `MAJOR.MINOR.PATCH`:

- **MAJOR** - any breaking change to a token name, a component selector, a slot name, or a required prop. Bumping MAJOR is a coordinated change between `docs/penpot/tokens.json`, `.specs/2 - visual-design-system/design.md`, and the Angular `@theme` block + components.
- **MINOR** - any additive change (new component, new variant, new size, new tone). Bumping MINOR is allowed per-release as long as the new asset is added to `docs/penpot/manifest.json`.
- **PATCH** - any non-breaking change to color values, radii, shadow alpha, motion timing. A PATCH is the most common kind of design tweak.

The current version is `0.1.0` (see `docs/penpot/design-system.penpot.json` and `docs/penpot/tokens.json` top-level `version` keys). The version is bumped in three places, atomically, in the same PR:

1. `docs/penpot/design-system.penpot.json` -> `version` key.
2. `docs/penpot/tokens.json` -> `version` key.
3. `docs/penpot/manifest.json` -> `version` key.

Add a CHANGELOG entry under `docs/penpot/CHANGELOG.md` (not yet present - created on the first non-trivial change) with the date, the new version, and a one-line description of the change.

## Spec gaps and conflicts discovered

While reading `.specs/2 - visual-design-system/` and `.specs/1 - modernization-roadmap/requirements.md` to produce this hand-off, the following gaps were noticed. They are out of scope for the Penpot handoff but should be tracked.

1. **`docs/design-system/README.md` does not exist yet.** Phase F of `.specs/2 - visual-design-system/tasks.md` plans to add it (task F.1). The Penpot hand-off points at it, but the README itself is a separate work item.
2. **No `src/Web/ControlEasyReborn.Web/` Angular project yet.** All token values are inlined into the SVGs as resolved hex/px from the light-theme block. Once the Angular app is scaffolded (task A.1), the `@theme` block in `styles.css` must be diffed against `docs/penpot/tokens.json` per the procedure above.
3. **`manifest.json` is not yet referenced by `docs/design-system/README.md`.** Once that README lands (task F.1), it should link to `docs/penpot/manifest.json` as the index of design assets.
4. **No icon library SVG yet.** Per `.kilo/agents/agents/FrontendAgent/AGENTS.md`, icons come from Lucide (`lucide-angular`). The component SVGs use simple line glyphs as placeholders. A future revision of this hand-off should reference `lucide-angular` icon names verbatim (e.g. `name="check"`, `name="home"`) and the SVGs should be updated to use the actual Lucide SVG paths.

## Visual regression note (Chrome availability)

A visual regression pass against the produced SVGs was not run in Chrome (Playwright browser tools were not invoked in this session - the task scope was asset production, not UI verification). The SVGs were authored to match the resolved token values in `docs/penpot/tokens.json` line by line, so a manual review is sufficient for a v0.1.0 first cut. The Phase E verification gate (Playwright snapshots of the showcase page, axe-core scan) is the proper regression point once the Angular showcase page exists.
