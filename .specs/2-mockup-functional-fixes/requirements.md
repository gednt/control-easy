# Mockup functional fixes — Requirements

The `mockup/` HTML/JS/CSS prototype is the visual reference for the Angular SPA, but a static review of the source
shows that several interactive components are non-functional: modals and dropdowns render visible by default,
filters and pagination don't react, the login form has no real error path, the topbar search is unwired, and a
duplicate `class` attribute drops a layout margin. The mockup must demonstrate the *behavior* of every component it
shows, not just the styling.

## User stories

- **UC-1:** As a reviewer, when I open `app.html`, I should **not** see the "Add resident" modal or any dropdown
  menu open at page load.
- **UC-2:** As a reviewer, when I click the "Add resident" button, the modal must appear with a backdrop, focus
  the first field, and close on the close button / Escape / clicking the backdrop.
- **UC-3:** As a reviewer, when I click any dropdown trigger (topbar user menu, "Block: All", "Sort: Name", row
  "⋯" action), the menu must toggle open/closed, close on outside click and Escape, and update its label when an
  item is picked.
- **UC-4:** As a reviewer, when I type in the topbar search or the "Search by name, apartment, or CPF…" input,
  the resident table must filter rows client-side (case-insensitive substring) and the pagination footer must
  update to "Showing X–Y of Z" where Z is the filtered count.
- **UC-5:** As a reviewer, when I click the "All / Active / Pending / Overdue" tabs, the table must filter
  accordingly and the count badge in each tab must update.
- **UC-6:** As a reviewer, when I change the "Sort" dropdown to "Apartment" or "Last visit", the table must
  re-sort.
- **UC-7:** As a reviewer, when I click page 2/3 in the pagination, the table must show rows 11–20 / 21–30 of the
  filtered set, the active page button highlight must move, and Prev/Next buttons must enable/disable
  accordingly.
- **UC-8:** As a reviewer, when I submit the login form, the submit button must show a spinner with
  `aria-busy="true"`. The canonical demo credentials are **`test@test.com` / `test123`** and must land on
  `app.html`. With any other email whose **password is shorter than 6 chars**, validation must fail. With
  `email == "fail@x.com"` (any password) I should see the inline error region with a 401-style message and
  the form re-enabled. With `email == "ratelimit@x.com"` (any password) the error must say "Too many
  attempts" and the submit button must disable for 15 seconds. With an email containing "multi" (e.g.
  `multi@test.com`) the tenant picker must appear after success.
- **UC-9:** As a reviewer, when I click the eye icon on the password field, the input type must toggle and the
  icon must swap.
- **UC-10:** As a reviewer, when I click "Use a different account" on the tenant picker, I must return to the
  empty login form with prefilled email cleared if "Remember me" was unchecked.
- **UC-11:** As a reviewer, when I click the "Forgotten password" link, a toast must say
  "Check your email for reset instructions" (demo behavior).
- **UC-12:** As a reviewer, when I click the "Trigger success / warning / error toast" buttons on `app.html`,
  the corresponding toast must appear top-right and auto-dismiss after 5 s.
- **UC-13:** As a reviewer, when I open `index.html`, the "Dashboard preview" stat grid must have its bottom
  margin (no missing `mb-8`).
- **UC-14:** As a reviewer on `app.html`, the breadcrumb "Início" must point to `index.html` (the design-system
  home), not the same page.
