# Mockup smoke checklist

Serve the mockup locally and walk through these checks. Each item maps 1:1 to a success criterion
in `.specs/2-mockup-functional-fixes/design.md`.

## How to serve

```bash
cd mockup
python -m http.server 8765 --bind 127.0.0.1
# Open http://127.0.0.1:8765/index.html
```

(Or just open `mockup/index.html` directly — everything is build-free.)

## Pages

- `index.html` — design-system home
- `app.html` — residents list with search, tabs, filters, pagination, modal, toasts
- `showcase.html` — all components in one place
- `login.html` — sign-in with `test@test.com` / `test123`

## Checklists

### 1. Page-load correctness

- [ ] On `app.html`, no modal is visible at page load.
- [ ] On `app.html`, no dropdown menu is visible at page load (user menu, "Block: All", "Sort: Name", row "⋯").
- [ ] On `showcase.html`, none of the four demo modals is visible at page load.
- [ ] On `index.html`, the "Dashboard preview" stat grid has its bottom margin (visible space below the 4 stat tiles).

### 2. Modals

- [ ] Click "Add resident" → modal opens, focus is in the first field (Full name).
- [ ] Press `Escape` → modal closes, focus returns to the "Add resident" button.
- [ ] Click the X → modal closes.
- [ ] Click on the dark backdrop (outside the white card) → modal closes.
- [ ] In the modal, click "Cancel" → modal closes.
- [ ] In the modal, click "Add resident" → success toast appears top-right, modal closes.

### 3. Dropdowns

- [ ] Click the user-avatar icon in the topbar → user menu opens.
- [ ] Click outside the menu → it closes.
- [ ] Click the "Block: All" filter → menu opens, pick "Block A" → menu closes, label changes to "Block: A", table re-renders to show only Block A residents.
- [ ] Click the "Sort: Name (A→Z)" filter → menu opens, pick "Last visit (newest)" → label changes, table re-orders by lastVisitAt descending.
- [ ] Hover over a row, click its "⋯" → row menu opens. Click outside → it closes.
- [ ] Press `Escape` while a dropdown is open → it closes.

### 4. Search

- [ ] Type `maria` in the topbar search → table filters to one row ("Maria Silva"), the pagination footer reads `Showing 1–1 of 1 results`.
- [ ] Type `maria` in the inline "Search by name, apartment, or CPF…" input → same result, and the topbar search field also syncs to "maria".
- [ ] Type `999.999.999-99` (or any other unmatched string) → table shows the empty state, "Clear filters" button is visible.
- [ ] Click "Clear filters" → search clears in both inputs, block resets to "Block: All", sort resets to "Name (A→Z)", tab resets to "All", table re-renders with the first 7 of 30 rows.

### 5. Tabs

- [ ] Click "Active" tab → table shows only active residents, "Showing 1–7 of N" where N matches the count shown in the tab label.
- [ ] Click "Overdue" → table shows only overdue residents.
- [ ] The tab counts in the labels update after a search (e.g. search `maria` → Active count = 1, others = 0).

### 6. Pagination

- [ ] On page 1, "Prev" and "First" are disabled.
- [ ] Click "2" → table shows rows 8–14, "Showing 8–14 of 30 results", "Prev" is enabled, page 2 is highlighted.
- [ ] Click "Next" → goes to page 3. Click "Last" → goes to page 5 (since 30 / 7 = 5 pages).
- [ ] On page 5, "Next" and "Last" are disabled.

### 7. Login form

- [ ] Submit empty form → inline error "Please enter your email and password."
- [ ] Type a 3-character password → inline error "Password must be at least 6 characters."
- [ ] Type `test@test.com` / `test123` → 900 ms loading spinner → navigates to `app.html`.
- [ ] Type `fail@x.com` / `whatever` → 900 ms loading → inline error "Invalid email or password. Please try again.", button is re-enabled.
- [ ] Type `ratelimit@x.com` / `whatever` → 900 ms loading → inline error "Too many attempts. Try again in 15 seconds.", button label counts down from "Try again in 15s" to "Try again in 1s" and re-enables at 0.
- [ ] Type `multi@test.com` / `whatever` → 900 ms loading → tenant picker appears with two options. Click "Use a different account" → returns to the empty form.

### 8. Password show/hide

- [ ] Click the eye icon in the password field → input switches to text, icon changes to "eye off".
- [ ] Click again → switches back to password.

### 9. Toasts

- [ ] Click "Trigger success toast" → green toast top-right, auto-dismisses after 5 s, X also dismisses immediately.
- [ ] Click "Trigger warning" → amber toast.
- [ ] Click "Trigger error" → red toast, `role="alert"`.
- [ ] On `login.html`, click "Forgot password?" → info toast "Check your email for reset instructions." appears; URL does not change to `#`.

### 10. Theme

- [ ] Click the sun/moon button in the topbar → theme switches light ↔ dark (and through "system" on the third click). The icon swaps.
- [ ] Reload the page → theme preference is remembered.
- [ ] On `login.html`, the floating sun/moon button works.

### 11. Mobile drawer

- [ ] Resize the window to < 640 px wide → the sidebar becomes a drawer.
- [ ] Click the hamburger button → drawer slides in, dark backdrop appears.
- [ ] Click outside the drawer → it closes.

## Demo credentials

| Email              | Password   | Outcome                                 |
| ------------------ | ---------- | --------------------------------------- |
| `test@test.com`    | `test123`  | Success → `app.html`                    |
| `multi@test.com`   | anything   | Tenant picker                           |
| `fail@x.com`       | anything   | 401-style inline error                  |
| `ratelimit@x.com`  | anything   | 429-style inline error + 15 s cooldown  |
| anything else      | ≥ 6 chars  | Success → `app.html` (demo only)        |
