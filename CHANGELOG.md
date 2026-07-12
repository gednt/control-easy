# Changelog

## [Unreleased]

### Visual Design System (Phase 2)

- **CR-1 Resolved:** Adopted Tailwind CSS v4 + custom design tokens + Angular CDK (headless only). Dropped `@angular/material`. See `.specs/2 - visual-design-system/tasks.md`.
- Added 17 base components with `ce-` prefix: button, card, input, stat-tile, badge, modal, toast, table, empty-state, spinner, avatar, tabs, dropdown, tooltip, pagination, breadcrumbs, checkbox.
- Added `ThemeService` with `light`/`dark`/`system` mode, localStorage persistence, FOUC prevention.
- Created CSS token files for colors, radii, spacing, typography, shadows, and motion.
- Added `@theme` block in `styles.css` mapping tokens to Tailwind v4 utilities.
- Added `prefers-reduced-motion` global rule collapsing all animations.
- Added `:focus-visible` global outline rule.
- Added FOUC-prevention inline script in `index.html`.
- Added `/design-system/showcase` route rendering all components in both themes.
- Added i18n setup: `@angular/localize` with `pt-BR` source locale.
- Added `clsx` for class composition.
- Added `proxy.conf.json` for API dev proxy.
- Added ESLint + Prettier configuration.
- Showcase URL: `/design-system/showcase`