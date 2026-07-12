# Requirements — Tenant Administration UI

## UC-1: List condominiums

As a **platform operator**, I want to see all registered condominiums (tenants) with slug, display name, and status so I can manage the fleet from the web UI.

## UC-2: Register a new condominium

As a **platform operator**, I want to register a new condominium with a unique slug and display name so onboarding does not require running SQL or calling the API manually.

## UC-3: Create the first tenant admin

As a **platform operator**, I want to optionally create the first `TenantAdmin` during condominium registration (email, display name, temporary password) so the condominium can be handed off immediately.

## UC-4: Suspend and resume condominiums

As a **platform operator**, I want to suspend or resume a condominium from the UI so I can disable access without deleting data.

## UC-5: PlatformAdmin-only access

As a **security officer**, I want tenant administration screens and APIs restricted to users with the `PlatformAdmin` role so tenant-scoped users cannot provision new condominiums.

## UC-6: First-boot continuity

As a **new operator** who completed PlatformAdmin first-boot login, I want a clear navigation entry to condominium management so I know what to do after changing the bootstrap password.
