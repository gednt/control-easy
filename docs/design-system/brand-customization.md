# Brand Customization

## The `--color-primary-raw` hook

All brand-specific colors in the design system are derived from a single variable: `--color-primary-raw`. This variable is set in the light and dark themes, and all other primary colors (`--color-primary`, `--color-primary-hover`, `--color-primary-light`, `--shadow-primary-glow`) reference it.

### Override per tenant

To change the brand color for a specific tenant, add a `data-tenant` attribute to `<app-root>`:

```css
[data-tenant="acme"] {
  --color-primary-raw: #059669; /* emerald for Acme */
}

[data-tenant="globex"] {
  --color-primary-raw: #db2777; /* pink for Globex */
}
```

This is the exact CSS line: `src/app/design-system/tokens/colors.css`, line 9 (`--color-primary-raw: #4f46e5;`).

### How it cascades

1. `:root { --color-primary-raw: #4f46e5; }` — default indigo
2. `:root { --color-primary: var(--color-primary-raw); }` — all utilities use `var(--color-primary)`
3. `[data-tenant="acme"] { --color-primary-raw: #059669; }` — override just the raw value
4. Tailwind's `bg-primary` and `text-primary` resolve to `var(--color-primary)` → `var(--color-primary-raw)` → the tenant's color

No other changes are needed. The entire app re-colors automatically.

### Future: TenantBrandService

A future spec will add `TenantBrandService` that:
- Reads the current tenant ID from `AuthService`
- Sets `data-tenant` on `<app-root>`
- Optionally overrides `--color-primary-raw` via a runtime CSS variable