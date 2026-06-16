# Requirements: Mockup Visual Parity

## Overview

The ControlEasy Reborn Angular SPA at `src/Web/ControlEasyReborn.Web/` has a solid design-system foundation (tokens, `ce-*` components, Tailwind v4 utilities), but the **feature pages and shell** still bypass the design system and use Unicode glyphs where the mockup uses Lucide icons. The canonical visual reference lives in [`mockup/`](../../mockup/) (HTML pages) and [`docs/penpot/`](../../docs/penpot/) (Penpot JSON + SVG library).

This spec brings the Angular UI in line with the mockup for the app shell, login, residents reference page, and showcase while keeping English UI copy. It also addresses a follow-up item from `.specs/fix-design-system/requirements.md` (UC3 out-of-scope): the seven feature pages that still duplicate design-system component CSS.

## Glossary

- **`mockup/`** — Build-free HTML/CSS/JS prototype at the repo root. The four pages are `index.html` (hub), `app.html` (residents list), `showcase.html` (component gallery), and `login.html` (auth). Served via `python -m http.server 8765` from `mockup/`.
- **`docs/penpot/`** — Penpot design library, machine-readable tokens (`tokens.json`), 17 component SVGs, 5 screen SVGs, and `manifest.json`. Source of truth for the visual contract.
- **Lucide** — Open-source line-icon library. Used in mockup via [`mockup/assets/icons.js`](../../mockup/assets/icons.js). In Angular, the `lucide-angular` package is in `package.json` but not yet imported.
- **`ce-*` components** — The Angular design system (`button`, `card`, `input`, `stat-tile`, `badge`, `modal`, `toast`, `table`, `empty-state`, `spinner`, `avatar`, `tabs`, `dropdown`, `tooltip`, `pagination`, `breadcrumbs`, `checkbox`). Barrel export at `src/app/design-system/index.ts`.
- **Token namespace** — `var(--space-*)` is the canonical spacing scale; `var(--spacing-*)` is a Tailwind utility alias only (resolved in `.specs/fix-design-system/tasks.md`).
- **App shell** — The chrome composed of `AppShellComponent` (CSS grid), `SidebarComponent` (left nav), and `TopbarComponent` (top bar).
- **Mobile drawer** — The `<640px` slide-in variant of the sidebar. Mockup has explicit `position: fixed`, `transform: translateX(-100%)`, and a dark backdrop.
- **Visual parity** — Pixel-comparable appearance in light and dark themes at 375px, 768px, and 1440px viewports.
- **Recent activity** — A `ce-card` section in `mockup/app.html` showing `dashboard.recentVisits` styled as a list (used here on the Residents page header).

## User Stories

### UC1: Shared icon component

As a developer, I need a single `ce-icon` component backed by Lucide so that every icon in the app uses the same library, naming, and stroke style.

**Current problem:** `lucide-angular` is installed but unused. Sidebar, topbar, login, and residents pages use raw HTML entities (`&#9783;`, `&#128269;`, etc.) which render inconsistently across browsers and break in some font stacks.

### UC2: Mobile drawer behavior

As a user on a small screen, I need the sidebar to slide in as a drawer with a dark backdrop so that I can reach the navigation without losing screen space.

**Current problem:** The hamburger button in the topbar calls an empty `toggleMobileMenu()` method. The sidebar is `display: none` below 640px and cannot be opened.

### UC3: Topbar parity

As a user, I need the topbar to look and behave like the mockup: breadcrumbs, global search, notifications bell, theme toggle, and an avatar-driven profile menu.

**Current problem:** The topbar renders only breadcrumbs, a tenant `<select>`, theme toggle, and a custom profile menu. The mockup adds a debounced search input, a bell with tooltip, and a richer profile menu.

### UC4: Login refactor

As a developer, I need the login and change-password pages to use the design-system `ce-input`, `ce-checkbox`, and `ce-button` components so that input prefix/suffix icons, password show/hide, and error states work consistently.

**Current problem:** Both pages still duplicate `.ce-input-*`, `.ce-button`, `.ce-checkbox-*`, and `.ce-spinner` CSS rather than importing the components. The fix-design-system spec (UC3) explicitly left this for a follow-up.

### UC5: Residents page parity

As a user, I need the Residents page to match the mockup: stat tiles, status tabs, block and sort filters, sortable table, pagination, row action menu, add/edit/deactivate modals, and a recent-activity card.

**Current problem:** The page shows a single search input and a flat table. Stat tiles, tabs, filters, pagination, and recent activity are missing. The "Add resident" and "Edit" modals are inline templates with hand-written styles instead of `CeModalComponent`.

### UC6: Cross-page component adoption

As a developer, I need every feature page to import `ce-*` components instead of duplicating their styles, so the codebase has a single visual source of truth.

**Current problem:** `visits`, `vehicles`, `apartments`, `service-providers`, `administration`, and `condominiums` pages each duplicate `.ce-button`, `.ce-table`, `.ce-input-*`, `.ce-badge`, and `.ce-card` rules. Removing them shrinks CSS and prevents drift.

### UC7: Dashboard and showcase alignment

As a user, I need the dashboard stat grid to match the mockup and the showcase page to render every component exactly as the standalone `mockup/showcase.html` does.

**Current problem:** Dashboard duplicates `.ce-button` and `.ce-card` styles. The showcase page may have variant/size drift from the mockup gallery.

### UC8: Visual regression coverage

As a maintainer, I need Playwright snapshots of `/login`, `/residents`, and `/` (dashboard) at light/dark and 375/1440 so that future changes are caught before merge.

**Current problem:** Only `/design-system/showcase` is snapshotted. The `mockup/SMOKE.md` checklist is manual-only.

### UC9: Complete prior verification tasks

As a maintainer, I need the four unchecked items in `.specs/fix-design-system/tasks.md` (12.5–12.8) to be done so the token unification is fully verified.

**Current problem:** Tasks 1–11 are done; 12.5–12.8 (visual verification of showcase in both themes, login/change-password component use, modal/dropdown/tabs/breadcrumbs functional behavior, dark theme sidebar hover) are not yet ticked off.
