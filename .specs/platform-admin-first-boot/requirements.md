# PlatformAdmin First-Boot Login — Requirements

## User stories

- **UC-1:** As a **new operator** starting the normal Docker stack, I want the bootstrapped PlatformAdmin credentials from API logs to work on the login page so I can complete first boot without demo mode.
- **UC-2:** As a **security officer**, I want bootstrap to remain idempotent (skip when PlatformAdmin already exists) and still force password change on first login (`MustChangePassword = true`).
- **UC-3:** As a **developer**, I want an automated test proving PlatformAdmin login succeeds on a non-demo stack after bootstrap so regressions are caught in CI.
