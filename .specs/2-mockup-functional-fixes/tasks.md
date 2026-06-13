# Mockup functional fixes — Tasks

Each task has a verification gate. Tasks in the same wave have no dependencies on each other and can run in
parallel.

## Wave 1 — Foundation (no cross-dependencies)

- [ ] 1. **Add CSS visibility rules for modal and dropdown.** In `mockup/styles.css`:
  - Change `.ce-dropdown-menu` to include `display: none;` and add a new rule
    `.ce-dropdown-menu.is-open { display: block; }`.
  - Change `.ce-modal-backdrop` to include `display: none;` and add
    `.ce-modal-backdrop.is-open { display: flex; align-items: center; justify-content: center; }`.
  - Move the `animation: fade-in` to `.ce-modal-backdrop.is-open` so it only plays on open.
  - Verification: `grep -n "display: none" mockup/styles.css` shows the new rule on `.ce-dropdown-menu`.
  - Verification: `grep -n "is-open" mockup/styles.css` shows the new show rules.

- [ ] 2. **Fix duplicate `class` attribute in `mockup/index.html`.** Merge the two `class` attributes on the
  Dashboard-preview stat grid (`<div class="grid" style="..." class="mb-8">`) into
  `<div class="grid mb-8" style="...">`.
  - Verification: `grep -n 'class="[^"]*" .*class="[^"]*"' mockup/index.html` returns no results.

- [ ] 3. **Fix the breadcrumb href on `mockup/app.html`.** The first breadcrumb item currently points to
  `app.html`; change it to `index.html`.
  - Verification: `grep -n 'href="app.html"' mockup/app.html` returns only the sidebar's intentional self-links,
    not the breadcrumb.

- [ ] 4. **Remove the redundant `removeAttribute('hidden')` script on `mockup/app.html`.** This line was a
  workaround for the broken CSS visibility — once task 1 lands, the script is a footgun (it explicitly forces
  modals to ignore `hidden`).
  - Verification: `grep -n "removeAttribute(.hidden" mockup/app.html` returns no results.

- [ ] 5. **Remove the same workaround on `mockup/showcase.html`.** The block
  `m.removeAttribute("hidden"); m.hidden = true;` is replaced with nothing — CSS now handles it.
  - Verification: `grep -n "removeAttribute" mockup/showcase.html` returns no results.

- [ ] 6. **Add `data-toast` to the "Forgot password?" link on `mockup/login.html`.** Add
  `data-toast="info" data-toast-msg="Check your email for reset instructions."` to the `<a class="login-forgot">`
  element. Also add `href="#"` is already there, but `e.preventDefault()` must be added in the click handler
  (it is in `app.js` toast handler? No — that handler is on `data-toast` *buttons*, not anchors; we need to
  prevent the default for `<a>` tags. Update `app.js` toast handler to call `e.preventDefault()` when the
  trigger is an `<a>`).
  - Verification: `grep -n "login-forgot" mockup/login.html` shows the new attributes.

- [ ] 7. **Create `mockup/assets/data.js`.** A single global `window.MockData` exporting:
  - `residents` — array of 30 records: `{ id, name, cpf, block, apartment, phone, status, lastVisitAt }`,
    where `status ∈ {active, pending, overdue, inactive}` and `lastVisitAt` is an ISO string.
  - `filterSortPaginate(residents, { search, block, status, sortKey, page, pageSize })` — pure function
    that returns `{ rows, total, page, pageSize, pageCount }`. `sortKey ∈ {nameAsc, nameDesc, apartment,
    lastVisitDesc}`.
  - `escapeHtml(s)` — used in the row-render template to avoid XSS from arbitrary strings.
  - Verification: the file is loaded before `app.js` in all four HTML files (we add the `<script>` tag).

- [ ] 8. **Robustness tweak in `mockup/assets/icons.js`.** Augment `Icons` with an `IntrinsicSize` lookup so
  `icon(name, size)` always works. Behaviour is unchanged for existing 16/20/24 icons; new ones with arbitrary
  size also work.
  - Verification: `node -e "require('./mockup/assets/icons.js')"` succeeds; manual `window.icon('dashboard', 18)`
    returns a string with `width="18"`.

## Wave 2 — Data wiring (depends on Wave 1's CSS + data.js)

- [ ] 9. **Add the resident dataset script tag to all four HTML files.** Insert
  `<script src="assets/data.js"></script>` after `icons.js` and before `app.js` in `mockup/index.html`,
  `mockup/app.html`, `mockup/showcase.html`, `mockup/login.html`.
  - Verification: `grep -n "assets/data.js" mockup/*.html` returns four lines.

- [ ] 10. **Render the resident table from data in `mockup/app.html`.** Add `id="resident-tbody"` to the
  `<tbody>` of the resident table and a placeholder `<tr id="resident-empty">` hidden by default with the
  "No residents match your filters" empty state. JS will replace the rows.
  - Verification: `grep -n "resident-tbody" mockup/app.html` returns a match.

- [ ] 11. **Add `data-*` attributes to the filter triggers on `mockup/app.html`.** Wrap the trigger text in
  a `<span class="filter-label">` (so JS can rewrite it) and add `data-filter="block"` /
  `data-filter="sort"` and `id="filter-block-label"` / `id="filter-sort-label"`. Add `data-status` to each tab.
  - Verification: `grep -n "data-filter" mockup/app.html` returns matches.

## Wave 3 — Behavior (depends on Wave 2)

- [ ] 12. **Wire search debounce, tab status, block filter, sort, and pagination in `mockup/assets/app.js`.**
  - State held in a module-scope `state = { search, block, status, sortKey, page }`.
  - `applyAndRender()` calls `MockData.filterSortPaginate(...)` and re-renders the table and pagination
    footer, updates tab counts, and marks the active page button.
  - `[data-search]` (topbar + inline) get an `input` listener with 120 ms debounce.
  - Tab clicks call `setState({ status, page: 1 })` and re-render.
  - Dropdown items inside `[data-filter="block"]` / `[data-filter="sort"]` set the matching key on click.
  - Pagination buttons (page numbers, Prev, Next, First, Last) call `setState({ page })`.
  - Verification: the existing pagination test in `app.js` (which rewrites `.ce-pagination-info`) is
    removed/replaced by a single source of truth (`applyAndRender`); `grep -n "Showing" mockup/assets/app.js`
    shows it in the template string, not in ad-hoc handlers.

- [ ] 13. **Wire login error and rate-limit paths in `mockup/assets/app.js`.** Order of evaluation in the
  submit handler:
  1. Empty email or password → validation error.
  2. `password.length < 6` → validation error.
  3. `email.toLowerCase() === "fail@x.com"` → after 900 ms loading, show inline error
     "Invalid email or password. Please try again." and re-enable the button.
  4. `email.toLowerCase() === "ratelimit@x.com"` → after 900 ms loading, show inline error
     "Too many attempts. Try again in 15 seconds." and disable the submit button for 15 s; re-enable on
     `setTimeout`.
  5. `email === "test@test.com" && password === "test123"` → after 900 ms, navigate to `app.html`.
  6. `email.toLowerCase().includes("multi")` → after 900 ms, show tenant picker.
  7. Otherwise → after 900 ms, navigate to `app.html` (demo only).
  - Verification: each branch is reached with the expected email; submit button state and error text differ
    by branch.

- [ ] 14. **Make the "Forgot password?" link prevent navigation and surface a toast.** Update the
  `data-toast` click handler in `app.js` to call `e.preventDefault()` when the trigger is an anchor.
  - Verification: clicking the link does not change `location.hash`.

- [ ] 15. **Produce `mockup/SMOKE.md`** — a reviewer checklist that maps 1:1 to the success criteria in
  `design.md`. Include: "open app.html → no modal/dropdown visible", "click Add resident → opens", "search
  filters", "tab filters", "block/sort changes table", "pagination changes rows + footer",
  "login test@test.com / test123 navigates to app.html", "login fail@x.com shows inline error",
  "login ratelimit@x.com disables submit 15 s", "login multi in email shows tenant picker", "password eye toggles", "forgot password
  triggers toast", "dashboard preview margin correct".
  - Verification: file exists and contains 12+ checklist items.

## Task Dependency Graph

```json
{
  "waves": [
    {
      "wave": 1,
      "tasks": ["1", "2", "3", "4", "5", "6", "7", "8"]
    },
    {
      "wave": 2,
      "tasks": ["9", "10", "11"]
    },
    {
      "wave": 3,
      "tasks": ["12", "13", "14", "15"]
    }
  ]
}
```
