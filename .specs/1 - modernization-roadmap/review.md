# Code Review — Modernization Roadmap (Multi-Tenant + Attendant Profiles merge)

> Phase 3 of 3. Doc-only review of the Documentation Agent's merge of the
> Backend Agent's delta into `requirements.md`, `design.md`, and `tasks.md`.
> No implementation code exists yet; this review evaluates **internal
> consistency** of the merged docs against the delta
> (`tenant-and-attendant-deltas.md`) and the orchestrator's five flagged
> ambiguities (`orchestration.md` Phase 2 section).

## Summary

- **PR intent:** Fold multi-tenant capacity and multi-attendant profiles into
  the existing Modernization Roadmap spec by merging the Backend Agent's
  delta (data-isolation decision, `Tenants` module, `AttendantProfile`,
  request-pipeline changes, JWT claim shape, new tasks with acceptance
  criteria) into the three existing spec files.
- **Scope:**
  - `.specs/1 - modernization-roadmap/requirements.md` (48 → 56 lines)
  - `.specs/1 - modernization-roadmap/design.md` (456 → 553 lines)
  - `.specs/1 - modernization-roadmap/tasks.md` (108 → 159 lines)
- **Overall assessment:** **Request Changes**

  The merge is faithful in spirit and the five ambiguities the orchestrator
  flagged are mostly resolved, but one **Major** finding is a hard blocker:
  the original task 2.1 was kept in place while a `2.1 (updated)` sub-bullet
  was appended, producing two task-2.1 entries with **conflicting role
  names** (`Admin`, `Porteiro`, `Morador` vs. `PlatformAdmin`, `TenantAdmin`,
  `Morador`, `AttendantProfile`). This contradicts the very purpose of the
  merge and must be fixed before the spec is consistent.

## Findings

### Critical

*(none)*

### Major

- **M1. tasks.md:79 — Original task 2.1 left in place alongside `2.1 (updated)`.**
  - **Category:** correctness
  - **Description:** The Documentation Agent added the new role set and
    endpoints as a `2.1 (updated)` sub-bullet on line 80, but the original
    line 79 still reads: `role claims (`Admin`, `Porteiro`, `Morador`)`. The
    file now contains two competing task-2.1 definitions for the Security
    module — one with the old three-role model and one with the new
    `PlatformAdmin` / `TenantAdmin` / `Morador` / `AttendantProfile` model.
    A reader following the original line would implement the obsolete
    role set. The delta explicitly intended to *update* 2.1, not to
    preserve a parallel copy.
  - **Suggestion:** Replace the original line 79 with the updated text (or
    delete the original and renumber the updated sub-bullet up). See
    Follow-up Edits F1 below for the exact diff.

- **M2. tasks.md:29 vs. tasks.md:62-63 — 1.0b depends on a NetArchTest rule
  that 1.15 introduces.**
  - **Category:** correctness (ambiguity #1)
  - **Description:** Task 1.0b's body says it adds a NetArchTest rule
    "in 1.15" (line 29). The rule is documented as a sub-bullet of 1.15
    on lines 62-63. But in the Phase 1 task list, 1.0b ships before
    1.15 (1.0a → 1.0b → 1.0c → 1.1 → ... → 1.15). For the build to fail
    when a new `CREATE TABLE` is added *as part of 1.0b's own PR*, the
    NetArchTest rule must land in the same PR as 1.0b, not "in 1.15"
    (which is a later task in the same phase, but a later *commit* in
    straight-line execution). The current wording is ambiguous about
    this and risks a PR where the rule is not yet present to enforce
    1.0b's own acceptance criterion.
  - **Suggestion:** Reword 1.0b to say the rule ships "in the same PR as
    1.0b (added to the 1.15 test project which is also created in
    Phase 1)" — i.e., make it explicit that 1.0b and the 1.15
    sub-bullet land together. See Follow-up Edits F2 below.

- **M3. design.md:295 and tasks.md:90 reference `POST /api/v1/security/tenant-switch`
  but it is missing from the formal endpoint bullet list at design.md:311-319.**
  - **Category:** correctness
  - **Description:** The "Multi-tenant attendant support" prose (design.md:295)
    and task 2.1 (updated) in tasks.md:90 both promise a
    `POST /api/v1/security/tenant-switch` endpoint that re-issues a JWT for
    another tenant the user belongs to. The formal endpoint bullet list
    under "Attendant Profiles" (design.md:310-319) enumerates every other
    attendant/shift/gatehouse/tenant-picker endpoint but omits
    `tenant-switch`. The same is true for `GET /api/v1/security/tenants?email=...`
    *body shape* — the bullet mentions the response shape inline, which is
    fine, but the absence of `tenant-switch` is a gap. A reader
    implementing the endpoints list at lines 311-319 will not produce
    `tenant-switch`.
  - **Suggestion:** Add `POST /api/v1/security/tenant-switch` to the
    endpoint list at design.md:319 (new line). See Follow-up Edits F3
    below.

- **M4. design.md:310 — Endpoint intro line says "under `POST /api/v1/security/...`"
  but the list contains GETs.**
  - **Category:** correctness
  - **Description:** The endpoint bullet list at design.md:310 is
    introduced as "(under `POST /api/v1/security/...`)" but the list
    below it contains GETs, PUTs, and POSTs. This is a copy-paste from
    the delta header that is technically incorrect in the merged doc.
  - **Suggestion:** Reword to "under `/api/v1/security/...`" (drop the
    `POST`). See Follow-up Edits F3 below.

### Minor

- **m1. requirements.md:13 and :18 — duplicated `### Frontend` heading.**
  - **Category:** style
  - **Description:** Pre-existing typo (acknowledged in
    orchestration.md:93, "Typo note (separate edit, NOT for Phase 3)").
    The merge did not introduce it but did not fix it either. Per the
    orchestrator, this is out of scope for Phase 3. Recording for the
    next cleanup pass.

- **m2. design.md glossary (lines 13-34) — new terms are appended in the
  order from the delta, not alphabetically.**
  - **Category:** style (ambiguity #3)
  - **Description:** The original glossary (lines 13-28) is **not**
    alphabetical — it is grouped by category (architectural patterns, then
    library terms, then infrastructure terms). The new terms (lines 29-34)
    are appended in the order the delta wrote them (Tenant, AttendantProfile,
    ITenantContext, PlatformAdmin, TenantAdmin, TenantResolutionStrategy).
    This is consistent with the pre-existing convention (grouped, not
    alphabetical). The Documentation Agent's note in orchestration.md:90
    says "in alphabetical order, not re-sorted globally" — that note
    mis-describes what they did (they did not sort alphabetically; the
    pre-existing glossary wasn't alphabetical either). The result is
    internally consistent, but the rationale in the orchestration log
    is incorrect.
  - **Suggestion:** No doc change required. If the orchestrator wants the
    glossary to be strictly alphabetical going forward, that is a
    separate cleanup; do not change the merge.

- **m3. orchestration.md:33-34 — Change map still says "2.7" and "3.9"
  (Angular UI) but the merged tasks use "2.7a" and "3.9a".**
  - **Category:** documentation drift
  - **Description:** The orchestrator's change map describes the Phase 2
    addition as `2.7 Attendant profile management UI (Angular)` and the
    Phase 3 addition as `3.9 Tenant administration UI (Angular)`. The
    actual tasks in tasks.md are `2.7a` and `3.9a` (backend slices, with
    a future slot reserved for the Frontend-Agent UI tasks 2.7 / 3.9).
    The Phase 2 ambiguity log (line 91) explains the choice, so this is
    not a hard contradiction, but the change map is now slightly stale.
  - **Suggestion:** Reword the change map to read "2.7a (backend slice;
    2.7 reserved for a future Frontend Agent UI task)" and similarly for
    3.9a. See Follow-up Edits F4 below.

- **m4. design.md glossary — "Attendant" (the role string used in JWT
  claims) is not a glossary entry.**
  - **Category:** consistency
  - **Description:** The body of design.md, requirements.md (UC-9) and
    tasks.md (2.1 updated) uses the role string `Attendant` in
    `roles[]` (e.g. `["Attendant"]`). The glossary defines
    `AttendantProfile` (the data row) but not `Attendant` (the role
    string). A new reader could confuse the two. Not a blocker — the
    term is self-describing in context — but worth a one-line glossary
    entry for clarity.
  - **Suggestion:** Add a row to the glossary. See Follow-up Edits F5
    below.

### Nit

- **n1. design.md:247 — `PlatformAdminOnly` policy name is referenced
  in the prose but the formal list at design.md:327 uses
  `PlatformAdminOnly` (consistent). No change required.**

- **n2. design.md:319 — `userDisplayName` field name in the response
  shape uses lower-camel-case. The rest of the spec mostly uses
  lower-camel-case for JSON and PascalCase for C#. Consistent. No
  change required.**

## Positive Observations

- The merge is **spirit-faithful** to the delta. Every user story
  (UC-21..UC-25) lands in requirements.md, every Multi-Tenancy /
  Attendant Profiles / Tenant Data Migration subsection lands in
  design.md, and the new tasks (1.0a, 1.0b, 1.0c, 2.1 updated, 2.7a,
  3.9a, C.6) plus the 1.13/1.14 updates all land in tasks.md.
- The `ITenantContext` interface block (design.md:253-265) is preserved
  verbatim with the `Set` method, which is what the C.6 acceptance
  criteria depend on.
- All six Backend-Agent follow-ups (R1-R6) from orchestration.md:65-70
  are addressed by the Documentation Agent and recorded in
  orchestration.md:80-85. R4 (Frontend) and R5 (DevOps) are correctly
  deferred to future specs.
- The endpoint grouping under "Attendant Profiles" (design.md:310-319)
  carries the R2 rate-limit note inline at line 319.
- The C.6 NetArchTest rule is documented with both the
  `CrossTenant_.*` regex and the `Platform`-prefix exclusion — the
  `Platform*` exclusion correctly keeps the `Tenants` aggregate (which
  intentionally has no `TenantId`) out of the rule.
- No emojis anywhere in the three updated files (verified by script).
- No leftover references to `condominium_id` (verified by grep).
- The `Out of Scope` additions (Billing, Cross-tenant analytics) are
  present in requirements.md:55-56 and are not contradicted by
  design.md or tasks.md (no "billing" or "subscription" implementation
  tasks exist; the `PlatformAdmin` tenant admin endpoints are scoped to
  a single tenant and do not produce cross-tenant analytics).

## Checklist

- [x] Logic correctness (one major contradiction: M1)
- [x] Edge cases handled (default-tenant GUID, off-shift, cross-tenant 403)
- [x] Error handling follows conventions (ProblemDetails mapped in 1.0a, design.md:326)
- [x] No security vulnerabilities (rate-limit on tenant-picker noted; tenant-switch not in endpoint list — see M3)
- [x] No performance regressions
- [x] Tests cover new/changed behavior (1.13 updated, 1.14 updated, C.6, 1.0c, 2.7a acceptance)
- [x] Types and interfaces are correct
- [x] Naming and style match project conventions (one typo pre-existing — see m1)
- [x] No breaking changes without documentation/migration
- [x] Dependencies are appropriate and allowed
- [x] Documentation updated if applicable

## Per-Ambiguity Verdict

### Ambiguity 1 — 1.0b depends on a NetArchTest rule added in 1.15

**Verdict: Needs fix.** The merge preserved the delta's wording "in
1.15" for the NetArchTest rule. In the Phase 1 ordering, 1.15 ships
after 1.0b, so a strict reading would mean 1.0b's acceptance criterion
("NetArchTest rule fails the build if a `CREATE TABLE` is added to
`docker/mysql/init/` that is not listed in the backfill script") is
self-referentially unenforceable on the PR that lands 1.0b. The
wording needs to be made explicit that the rule and 1.0b land in the
same PR. See Finding M2 and Follow-up Edit F2.

### Ambiguity 2 — C.6 references the existing C.1 GitHub Actions `test` job

**Verdict: OK.** C.1 (tasks.md:151) lists the GitHub Actions jobs as
`lint`, `build`, `test`, `docker`, `smoke`. C.6 (tasks.md:159) says
"These rules run on every PR (in the existing C.1 GitHub Actions
`test` job)." The job name matches; no change required.

### Ambiguity 3 — Glossary sort order

**Verdict: Recommendation (no change to the merge).** The pre-existing
glossary (design.md:13-28) is grouped by category, not alphabetical.
The new terms (design.md:29-34) are appended in the order from the
delta. This is consistent with the pre-existing convention. The
Documentation Agent's self-description in orchestration.md:90 ("in
alphabetical order, not re-sorted globally") is inaccurate — they
were not added in alphabetical order, and the original glossary was
not alphabetical either — but the *result* is acceptable. No doc
change required. If the team wants strict alphabetical order going
forward, that is a separate cleanup.

### Ambiguity 4 — Task numbering 2.7a vs. 2.7

**Verdict: OK.** `2.7a` is in tasks.md:97 and there is no `2.7`. The
gap from `2.6` (line 96) to `2.7a` (line 97) is intentional and
preserves the slot for a future Frontend-Agent-owned `2.7`
(Angular UI). Same pattern is used for `3.9a` (tasks.md:121). No
cross-reference in the doc uses `2.7` or `3.9` as a concrete task
pointer, so the gap does not break anything. The only stale reference
is in the orchestrator's change map (orchestration.md:33-34), which
is a minor documentation drift in the log, not in the spec itself —
see Finding m3 and Follow-up Edit F4.

### Ambiguity 5 — Policy list in design.md

**Verdict: OK (Recommendation: keep as merged).** The Documentation
Agent updated the policy list at design.md:327 from
`AdminOnly, PorteiroOnly, ResidentSelfOrAdmin` to
`PlatformAdminOnly, TenantAdminOnly, ResidentSelfOrAdmin, plus
fine-grained RequirePermission("visits.checkin")`. This is consistent
with the new role set in the Multi-Tenancy subsection
(`PlatformAdmin`, `TenantAdmin`, `Morador`, `AttendantProfile`) and
with UC-23 (PlatformAdmin / TenantAdmin) and UC-9 (full role set).
The `RequirePermission(...)` addition matches the Attendant Profiles
permission model (design.md:291). No other reference in the doc still
uses the old `AdminOnly` / `PorteiroOnly` names. The change goes
slightly beyond the literal delta instructions (the delta did not
restate the policy list at the API Design level), but it is a
necessary consistency fix — the old names would have been
self-contradictory. Keep as merged.

## Blocker vs. Non-Blocker

**Blockers (must fix before the spec is internally consistent):**

- **M1** — Original task 2.1 left in place alongside `2.1 (updated)`,
  with conflicting role names (`Admin`, `Porteiro`, `Morador` vs.
  `PlatformAdmin`, `TenantAdmin`, `Morador`, `AttendantProfile`).
  This is a direct contradiction in the file and the heart of the
  merge — the very thing the spec was updated to fix.
- **M2** — 1.0b depends on a NetArchTest rule in 1.15; the wording
  needs to make it explicit that the rule and 1.0b land in the same
  PR, otherwise 1.0b's own acceptance criterion is unenforceable.
- **M3** — `POST /api/v1/security/tenant-switch` is promised in
  design.md:295 and tasks.md:90 but is missing from the formal
  endpoint bullet list at design.md:311-319. A implementer
  following the endpoint list will not produce the endpoint.

**Non-blockers (fix in the same pass, but the spec is not
fundamentally broken by them):**

- **M4** — design.md:310 intro line "under `POST /api/v1/security/...`"
  is wrong because the list contains GETs and PUTs. Easy wording
  fix.
- **m3** — Orchestration change map (orchestration.md:33-34) still
  says "2.7" / "3.9" instead of "2.7a" / "3.9a". Doc-log drift.
- **m4** — Glossary does not define the `Attendant` role string
  (only the `AttendantProfile` data row). Optional.

**Out of scope for this review (pre-existing, not introduced by the merge):**

- **m1** — Duplicated `### Frontend` heading in requirements.md:13/18.
  Pre-existing typo, explicitly out of scope per orchestration.md:93.

## Follow-up Edits for the Documentation Agent

### F1 (Major — M1) — tasks.md:79

**Before:**
```
- [ ] **2.1** Implement the `Security` module: `Users` entity, `POST /api/v1/auth/login` → JWT, `POST /api/v1/auth/refresh`, role claims (`Admin`, `Porteiro`, `Morador`).
  - **2.1 (updated):** Implement the `Security` module with the full role set (`PlatformAdmin`, `TenantAdmin`, `Morador`, plus `AttendantProfile` per tenant) and the JWT claim shape from the "Multi-Tenancy" subsection (`sub`, `tenant_id`, `profile_id`, `roles[]`, `permissions[]`). Endpoints added on top of the existing `POST /api/v1/auth/login` and `POST /api/v1/auth/refresh`:
```

**After:**
```
- [ ] **2.1** Implement the `Security` module with the full role set (`PlatformAdmin`, `TenantAdmin`, `Morador`, plus `AttendantProfile` per tenant) and the JWT claim shape from the "Multi-Tenancy" subsection (`sub`, `tenant_id`, `profile_id`, `roles[]`, `permissions[]`). Existing endpoints carried over: `POST /api/v1/auth/login`, `POST /api/v1/auth/refresh`. Endpoints added by the multi-tenant + attendant-profiles work:
```

(The remaining bullet list of endpoints on lines 81-91 stays unchanged.)

### F2 (Major — M2) — tasks.md:29

**Before:**
```
- [ ] **1.0b** **Default-tenant backfill + WPF compatibility shim.** Add `docker/mysql/init/02-tenants-seed.sql`, `03-tenant-backfill.sql`, `04-tenant-views.sql` (per the "Tenant Data Migration" subsection). Add `scripts/generate-tenant-backfill.sql` to regenerate the backfill file from a table list, and add a NetArchTest rule (in 1.15) that fails the build if a new business table is missing from the backfill list.
```

**After:**
```
- [ ] **1.0b** **Default-tenant backfill + WPF compatibility shim.** Add `docker/mysql/init/02-tenants-seed.sql`, `03-tenant-backfill.sql`, `04-tenant-views.sql` (per the "Tenant Data Migration" subsection). Add `scripts/generate-tenant-backfill.sql` to regenerate the backfill file from a table list, and add a NetArchTest rule (shipped in the same PR as 1.0b, added to the 1.15 test project as the 1.15 sub-bullet below) that fails the build if a new business table is missing from the backfill list.
```

### F3 (Major — M3, Minor — M4) — design.md:310 and :319

**Before (line 310):**
```
- **API endpoints** (under `POST /api/v1/security/...`, all JWT-bearer, all subject to `tenant_id` global filter except the cross-tenant tenant-picker lookup):
```

**After (line 310):**
```
- **API endpoints** (under `/api/v1/security/...`, all JWT-bearer, all subject to `tenant_id` global filter except the cross-tenant tenant-picker lookup):
```

**After (line 319, append a new bullet):**
```
  - `GET    /api/v1/security/tenants?email=...` — **public** (no auth), rate-limited to 10/min/IP, returns `[{ tenantId, slug, displayName, userDisplayName }]` for the email's known tenants; used by the login screen's tenant picker.
  - `POST   /api/v1/security/tenant-switch` — re-issues a JWT for a different tenant the authenticated user belongs to (body: `{ tenantId }`); user is identified by the current JWT, no password required, scoped to the user's known tenants.
```

### F4 (Minor — m3) — orchestration.md:33-34

**Before:**
```
- **Phase 2** — add **2.7 Attendant profile management UI** (Angular).
- **Phase 3** — add **3.9 Tenant administration UI** (Angular).
```

**After:**
```
- **Phase 2** — add **2.7a Attendant profile API surface (backend slice)**; **2.7 Attendant profile management UI (Angular)** is reserved for a future Frontend-Agent spec.
- **Phase 3** — add **3.9a Tenant administration API surface (backend slice)**; **3.9 Tenant administration UI (Angular)** is reserved for a future Frontend-Agent spec.
```

### F5 (Minor — m4) — design.md glossary, append a new row after line 33

**After the `TenantAdmin` row (line 33), insert:**
```
| **Attendant** | The role string used in the JWT `roles[]` claim for an attendant session (e.g. `["Attendant"]`). Distinct from `AttendantProfile`, which is the per-tenant data row that sources the role and permissions. |
```

## Verdict

**Request Changes.** The merge is high quality and spirit-faithful, but
three Major findings (M1, M2, M3) and one Minor wording fix (M4) must be
addressed before the spec is internally consistent. The five
ambiguities flagged by the Documentation Agent are resolved except for
#1 (which becomes Finding M2) and one cross-cutting endpoint listing
gap (Finding M3). After the Follow-up Edits F1-F3 (and the easy F4-F5)
are applied, the merged docs will be ready to hand off.
