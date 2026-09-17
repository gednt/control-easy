# Quickstart: Gatehouse Access and Visit Destinations Validation

## Prerequisites

- Work from the `feat/qr-entrance-exit-access` worktree.
- Docker Desktop is running and the required tenant, attendant, resident, apartment, and vehicle test data exists.
- Use a tenant administrator for credential administration and a gatehouse attendant with the new Access permission for operational flows.
- Run all application commands inside the project devcontainer or Compose containers; do not restore or run .NET/Node dependencies on the host.

## Start the feature stack

From the worktree, use the branch-specific Compose project name:

```powershell
$proj = 'ce-feat-qr-entrance-exit-access'
docker compose -p $proj -f docker/docker-compose.yml build api web
docker compose -p $proj -f docker/docker-compose.yml up -d --force-recreate api web db reverse-proxy
docker compose -p $proj -f docker/docker-compose.yml ps
```

Expected result: `api`, `web`, `db`, and `reverse-proxy` are healthy/running. Open the branch-specific local URL supplied by the worktree Compose configuration and sign in with the appropriate role.

## Validate the main flows

### 1. Issue and use a resident QR credential

1. As a tenant administrator, choose an active resident and issue a QR credential.
2. Confirm the printable QR value is shown once and no CPF/name/apartment is visible inside it.
3. As a gatehouse attendant, scan the code for entrance, then for exit.
4. Confirm each response completes within the 3-second target and the review screen shows two immutable events with subject, direction, QR method, attendant, and time.

### 2. Issue and use a vehicle QR credential

1. Issue a credential for an active vehicle.
2. Scan it at entrance and exit.
3. Confirm the event identifies the selected vehicle; if `OwnerResidentId` exists, the UI can show the linked resident only as needed for disambiguation.

### 3. Validate required visit destinations and automatic recovery

1. Create and update visitor and service-provider visits in the existing interface. Confirm an active apartment is required and that its block and unit are shown before confirmation; no successful flow displays destination pending.
2. Select a resident in the existing visit interface and in the new QR/manual-access interface. Confirm the same active apartment, block, and unit are recovered without manual entry.
3. Select an associated vehicle and confirm its destination is derived from its verified resident owner, otherwise from its registered apartment.
4. Try to submit a visit or access event for a resident/vehicle with no active destination. Confirm a correction-required result and no incomplete/pending record.
5. Change a resident's apartment after recording a visit/access event and confirm the historical event still shows its original destination snapshot.

### 4. Use protected manual lookup

1. With no QR code, search an active resident using a complete CPF, another registered identity document, name, apartment, and block in separate checks.
2. Verify name/apartment/block searches reject insufficiently specific text and that document results never show full document values.
3. Select the intended resident or a verified associated vehicle, choose entrance or exit, and confirm.
4. Confirm the resulting event is recorded as `manual_lookup` and is attributable to the attendant, while the lookup audit contains no raw query value.

### 5. Validate revocation, retry, and duplicate safety

1. Replace or revoke a credential as an administrator.
2. Retry its old QR value and confirm refusal within 30 seconds, an auditable lifecycle action, and no identity disclosure beyond the current tenant.
3. Submit the same scan attempt twice and confirm the second request returns the original result without a second event.
4. Scan the same valid code rapidly with a different attempt identity and confirm the duplicate-warning path requires an explicit decision and remains reviewable.

### 6. Validate policy and tenant isolation

1. Configure a tenant policy that requires the existing consent workflow for the subject category.
2. Confirm QR/manual flow returns `policy_action_required` rather than silently bypassing the policy.
3. From a second tenant, attempt the first tenant's QR and the same document/name search. Confirm no resident, vehicle, document, or credential identity is disclosed.

### 7. Confirm exclusions

1. Inspect the credential administration and gatehouse UI and generated API descriptions.
2. Confirm there is no facial enrollment, template, comparison, liveness, score, or photo-to-biometric path.
3. Confirm scanning records software access only and never exposes a physical gate unlock control.
4. Run the architectural and schema exclusion assertions explicitly:

    ```powershell
    dotnet test tests/ControlEasyReborn.ArchitectureTests/ControlEasyReborn.ArchitectureTests.csproj --nologo --filter "FullyQualifiedName~BiometricExclusion"
    ```

    All four AccessControlBiometricExclusionTests cases must pass, and the schema grep below must return zero matches:

    ```powershell
    Select-String -Path docker/mysql/init/12-access-control-schema.sql,docker/mysql/migrations/0009-access-control.sql -Pattern 'biometric_' -SimpleMatch
    ```

    Reference: see `docs/access-control.md#biometric-exclusion` for the full reservation rationale and the future-spec requirement.

## Automated verification

After implementation tasks are complete, run the prescribed checks from the project **devcontainer shell** (never from the host). The devcontainer is a Docker container and targets the same Compose engine; the runtime services remain the branch-specific Compose stack started above.

```powershell
dotnet test tests/ControlEasyReborn.UnitTests/ControlEasyReborn.UnitTests.csproj --nologo
dotnet test tests/ControlEasyReborn.IntegrationTests/ControlEasyReborn.IntegrationTests.csproj --nologo
dotnet test tests/ControlEasyReborn.ArchitectureTests/ControlEasyReborn.ArchitectureTests.csproj --nologo
Set-Location src/Web/ControlEasyReborn.Web
npm test -- --no-watch --browsers=ChromeHeadless
npm run e2e
```

Expected result: all test projects pass, the Access Control cross-tenant, append-only, destination-required, and destination-snapshot cases are green, and the gatehouse E2E exercises existing and new resident destination recovery, QR, manual lookup, revoke, and duplicate scenarios.

If the devcontainer configuration or final Compose migration runner changes, retain the same Docker-only test scope rather than running tooling on the host.
