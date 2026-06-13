# Requirements — Demo Mode

> Spec folder: `.specs/4 - demo-mode/`
> Goal: Provide a one-command, pre-populated ControlEasy Reborn stack with fixed demo credentials, realistic sample data across two condominiums, and clear UI indicators so sales, training, and local smoke testing never start from an empty database.

## User Stories

### Activation & Operations

- **UC-41:** As a *sales engineer*, I want to start a fully populated demo stack with a single Docker Compose command so I can show ControlEasy to a prospect without manual database setup or hunting for bootstrap passwords.
- **UC-42:** As a *developer*, I want demo mode to be **opt-in** (`Demo__Enabled=false` by default) so my normal local development stack is unaffected and integration tests do not depend on demo seed data.
- **UC-43:** As a *DevOps engineer*, I want a `docker-compose.demo.yml` overlay that sets demo environment variables, enables all web feature flags, and documents the well-known JWT signing key so the demo stack is reproducible on any machine with Docker.

### Credentials & Authentication

- **UC-44:** As a *demo user*, I want fixed, documented login credentials (`demo123` password for all demo personas) so I can sign in immediately without reading server logs for a random bootstrap password.
- **UC-45:** As a *sales engineer*, I want at least five demo personas (PlatformAdmin, TenantAdmin, Attendant, Morador, multi-tenant user) so I can demonstrate role-based access and the tenant picker without creating accounts manually.
- **UC-46:** As a *prospect*, I want the login page to offer one-click "Try a demo account" shortcuts (demo mode only) so I can explore the product without typing credentials.

### Sample Data

- **UC-47:** As a *sales engineer*, I want two demo condominiums prefixed with **`[Demo]`** (**`[Demo] Residencial Aurora`** and **`[Demo] Condomínio Parque Verde`**, slugs `demo-aurora` and `demo-parque-verde`) populated with **thematic residents** (Chaves, GTA, God of War Greek era + Atreus), plus visits, vehicles, service providers, attendants, gatehouses, and shifts so every major screen shows memorable sample data visually distinguishable from real tenants.
- **UC-48:** As a *product owner*, I want **61 demo residents** at `[Demo] Residencial Aurora` — **thematic pop-culture characters** from **Chaves** (20), **GTA** (20), and **God of War Greek era + Atreus** (21), with **Chaves and God of War characters who live together in canon sharing the same apartment** in the seed data, so demos are memorable, paginated lists feel realistic, and household groupings match fan expectations.
- **UC-49:** As a *Chaves fan*, I want canonical households preserved in the seed — **apt 8** (Florinda, Quico, Girafales), **apt 14** (Madruga, Chilindrina, Chiquinha), **Barril** (Chaves solo), **apt 18** (Maruxa, Serafim) — so the condominium layout mirrors the vila from the TV series.
- **UC-50:** As a *God of War fan*, I want **Atreus** included alongside Greek-era characters and co-dwelling with **Kratos** at **Sparta-1**, with **Hades** and **Persephone** sharing **Underworld-1**, so the demo reflects recognizable mythic households.
- **UC-51:** As a *gatehouse attendant (demo)*, I want recent activity timestamps to appear relative to "now" (e.g., "2 hours ago") so the dashboard feels alive even if the demo has been running for days.

### UI & Discoverability

- **UC-52:** As a *demo user*, I want a visible banner in the app shell when demo mode is active so I always know I am viewing sample data that may be reset.
- **UC-53:** As a *trainer*, I want a `/help/demo` page (demo mode only) listing all demo accounts, suggested walkthrough steps, and a link to reset instructions so onboarding sessions are repeatable.

### Reset & Safety

- **UC-54:** As a *platform operator*, I want a `POST /api/v1/demo/reset` endpoint (PlatformAdmin-only, demo mode only) and documented `docker compose down -v` procedure so demo data can be restored to a known baseline after exploratory changes.

---

## Out of Scope (for this spec)

- Hosting a public internet-facing demo SaaS (TLS, rate limiting at edge, WAF) — future DevOps spec; this spec delivers the local/hosted **mechanism**.
- Anonymized import of real legacy `controlEasyDB.db` data — separate data-migration spec.
- Demo mode for the legacy WPF app — web stack only.
- Synthetic MQTT/hardware simulation — deferred to `.specs/3 - photo-capture-hardware-integration/` follow-up.
- Billing, subscription, or trial-expiry flows.
- Automatic demo data generation via LLM — static curated fixtures only.
