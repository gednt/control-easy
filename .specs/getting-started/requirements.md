# Getting Started (First Boot) — Requirements

## User stories

- **UC-1:** As a **new operator**, I want a step-by-step first-boot guide so I can bring up ControlEasy Reborn on a clean machine without reading the entire codebase.
- **UC-2:** As a **developer**, I want to understand what runs automatically on first boot (database init, default tenant, PlatformAdmin bootstrap) so I know what to expect in logs and when credentials appear.
- **UC-3:** As a **security-conscious deployer**, I want clear instructions for retrieving the one-time PlatformAdmin password, changing it immediately, and setting a production JWT signing key before exposing the stack.
- **UC-4:** As an **evaluator**, I want the guide to distinguish the normal (empty) stack from demo mode and link to the dedicated demo documentation so I pick the right path on day one.
- **UC-5:** As an **operator troubleshooting a failed boot**, I want a troubleshooting section covering common first-boot failures (port conflicts, unhealthy database, missing credentials in logs, stale volumes).
- **UC-6:** As a **platform administrator after first login**, I want documented next steps (create tenants, assign tenant admins, verify API and UI endpoints) so the empty stack becomes usable for a condominium.
