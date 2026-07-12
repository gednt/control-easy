# Getting Started (First Boot) — Tasks

## Task Dependency Graph

```json
{
  "waves": [
    { "wave": 1, "tasks": ["1"] },
    { "wave": 2, "tasks": ["2", "3"] },
    { "wave": 3, "tasks": ["4", "5"] }
  ]
}
```

---

- [x] **1. Verify first-boot behavior against source**
  - [x] Confirm compose services, ports, and env vars in `docker/docker-compose.yml` and `docker/.env.example`.
  - [x] List MySQL init scripts that run on normal first boot (`00` through `09`; exclude demo-only `10`, `11`).
  - [x] Confirm `PlatformAdminBootstrapService` log message format and idempotency.
  - [x] Confirm login/AttendantProfile requirement and document current limitation.
  - **Verification gate:** Notes captured in `docs/getting-started.md` draft outline match source code.

- [x] **2. Write `docs/getting-started.md`**
  - [x] Prerequisites and optional configuration.
  - [x] Step-by-step first boot (compose up, wait for health, retrieve credentials).
  - [x] Service URL table and smoke checks.
  - [x] Post-bootstrap operator checklist (secrets rotation, tenant creation overview).
  - [x] Troubleshooting (ports, unhealthy db, stale volume, missing logs).
  - [x] Full reset procedure.
  - [x] "Evaluate with demo data" section linking to `docs/demo-mode.md`.
  - [x] See also links to ADRs and migration docs.
  - **Verification gate:** All commands and URLs match compose file; no aspirational endpoints.

- [x] **3. Update cross-references**
  - [x] `README.md` — link to `docs/getting-started.md` from local development section.
  - [x] `docs/demo-mode.md` — add See also link to getting-started.
  - **Verification gate:** Relative links resolve from repository root on GitHub.

- [x] **4. Smoke-test documented commands**
  - [x] Run documented compose command (or confirm stack already running and matches doc).
  - [x] Confirm `GET http://localhost:8080/api/v1/demo/info` behavior matches doc.
  - **Verification gate:** At least one documented verification command succeeds against running stack or is marked as requiring fresh volume.

- [x] **5. Final review**
  - [x] No emojis; consistent terminology with glossary.
  - [x] No duplication of demo persona tables (link instead).
  - [x] Changelog: N/A (internal docs only, no public API change).
  - **Verification gate:** Peer-readable manual completes first-boot story end-to-end on paper.
