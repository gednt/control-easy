# Theming Guide

## Overview

The theming system uses CSS custom properties (tokens) that change based on the `data-theme` attribute on `<html>`. A `ThemeService` manages theme persistence and resolution.

## ThemeService

```typescript
import { ThemeService } from '@app/design-system';

// In a component:
readonly themeService = inject(ThemeService);

// Toggle: light → dark → system → light
themeService.toggle();

// Set explicitly:
themeService.setTheme('dark');
```

## FOUC Prevention

`index.html` contains an inline `<script>` that:
1. Reads `localStorage["ce.theme"]`
2. If `"system"`, checks `matchMedia('(prefers-color-scheme: dark)')`
3. Sets `document.documentElement.dataset.theme` before any CSS renders

This prevents a flash of the wrong theme on page load.

## Writing token-respecting components

### Do:
```html
<button class="bg-primary text-white rounded-lg px-4 py-2">
  Save
</button>
```

### Don't:
```html
<button style="background: #4f46e5; color: #fff; border-radius: 12px;">
  Save
</button>
```

## Token file structure

| File | Purpose |
|---|---|
| `tokens/colors.css` | Color tokens (light + dark palettes, sidebar) |
| `tokens/radii.css` | Border radius values |
| `tokens/spacing.css` | Spacing scale (4px → 64px) |
| `tokens/type.css` | Font families, sizes, weights, line heights |
| `tokens/shadow.css` | Box shadows (light + dark) |
| `tokens/motion.css` | Duration, easing, keyframes, reduced-motion |

All tokens are imported into `styles.css` and mapped to Tailwind v4 utilities via the `@theme` block.