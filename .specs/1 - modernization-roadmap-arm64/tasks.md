# Tasks — Multi-Architecture Support (linux/amd64, linux/arm64, windows/arm64)

> Companion to `requirements.md` and `design.md` in this delta spec folder (`.specs/1 - modernization-roadmap-arm64/`). Sibling delta of `.specs/1 - modernization-roadmap/`. **This is a spec-only delta — no code, Dockerfile, Compose, or CI workflow is created by this delegation.** Implementation belongs to a later delegation after the user approves this delta.
>
> **Amends** the following items in the base spec (read in conjunction):
> - `tasks.md` Phase 1 — task **1.9** (multi-stage Dockerfiles) → see delta task **1.19**.
> - `tasks.md` Phase 1 — task **1.10** (`docker-compose.yml`) → see delta task **1.20**.
> - `tasks.md` Phase 1 — task **1.17** (Makefile) → see delta task **1.21**.
> - `tasks.md` Continuous — task **C.1** (GitHub Actions `build` / `docker` / `smoke`) → see delta task **1.22** and the Phase 1 verification gate amendment in delta task **1.25**.
>
> These cross-references are encoded inline as "(amends base task 1.X)" so the base spec files in `.specs/1 - modernization-roadmap/` are **not modified** by this delta.

---

## Phase 1A — Multi-architecture support (delta sub-phase)

*Goal: extend the Phase 1 deliverable so that the api and web images, the docker-compose stack, the local-dev Makefile, and the GitHub Actions CI all support `linux/amd64`, `linux/arm64`, and `windows/arm64` (the latter as a sibling tag) on **native** runners, with cosign-signed multi-arch manifests pinned by digest in production.*

This sub-phase is appended to the end of base spec Phase 1. It does not change the order of any base task. The Phase 1 verification gate of the base spec is **amended** (not edited) by delta task 1.25.

- [ ] **1.18** Author `docker/docker-bake.hcl` defining the multi-arch build matrix.
  - **Amends:** adds a new file referenced by the amended docker-compose flow (delta task 1.20) and the new CI workflow (delta task 1.22).
  - **Acceptance criteria:**
    - File exists at `docker/docker-bake.hcl`.
    - Defines a `_common` group with `platforms = ["linux/amd64", "linux/arm64"]`.
    - Defines `target "api"` and `target "web"` that inherit `_common` and tag with `ghcr.io/${GHCR_OWNER}/api:${IMAGE_TAG}` and `ghcr.io/${GHCR_OWNER}/web:${IMAGE_TAG}` respectively.
    - Defines `target "api-windows-arm64"` and `target "web-windows-arm64"` with `platforms = ["windows/arm64"]` and the `-windows-arm64` suffix tag.
    - Defines `group "default"` that builds all four targets in one `docker buildx bake` invocation.
    - `docker buildx bake -f docker/docker-bake.hcl --print` parses without error.
    - The `IMAGE_TAG` and `GHCR_OWNER` variables are overridable from the CLI (no hard-coded values).

- [ ] **1.19** Update `docker/api.Dockerfile` and `docker/web.Dockerfile` to be multi-arch safe.
  - **Amends:** base spec task **1.9** (multi-stage Dockerfiles).
  - **Acceptance criteria:**
    - `api.Dockerfile` build stage uses `FROM --platform=$BUILDPLATFORM mcr.microsoft.com/dotnet/sdk:8.0 AS build`; runtime stage uses `FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime` (no platform pin, so the multi-arch manifest picks the matching layer).
    - `web.Dockerfile` build stage uses `FROM --platform=$BUILDPLATFORM node:20-bookworm-slim AS build`; runtime stage uses `FROM nginx:1.27-alpine AS runtime`.
    - `ARG TARGETARCH` and `ARG TARGETOS` are declared in the api build stage; passed to `dotnet restore` and `dotnet publish` via `-a $TARGETARCH`.
    - `ARG TARGETARCH` is declared in the web build stage; documented as informational (Angular 18 has no native build step).
    - No `apt-get` / `apk add` / `npm install -g` step installs an amd64-only package; no hard-coded `x86_64` / `amd64` / `aarch64` strings in any `RUN` step that gates on architecture.
    - `docker buildx bake -f docker/docker-bake.hcl` builds both Linux and Windows targets against the Docker Hub / MCR base images and pushes them to a scratch registry; `docker buildx imagetools inspect` on each pushed image returns the expected platform list.
    - A documented verification step records the **base image manifest digest** of `mcr.microsoft.com/dotnet/sdk:8.0`, `mcr.microsoft.com/dotnet/aspnet:8.0`, `node:20-bookworm-slim`, `nginx:1.27-alpine`, `mysql:8.0`, `traefik:v3.1`, `adminer:4.8`, and `redis:7-alpine` (where applicable), and asserts that the manifest includes the required platforms. The digests are pasted into ADR 0004 (delta task 1.23).

- [ ] **1.20** Update `docker/docker-compose.yml` and `docker/.env.example` for native-host execution and `DOCKER_PLATFORM` opt-in.
  - **Amends:** base spec task **1.10** (`docker-compose.yml`) and the `docker/.env.example` block in the same task.
  - **Acceptance criteria:**
    - `docker/docker-compose.yml` does **not** set a top-level `platform:` key. Every service has a `platform: ${DOCKER_PLATFORM:-}` line (the variable defaults to empty, so the Docker Engine picks the host-native variant).
    - The dev override file `docker/docker-compose.override.yml` does **not** set any `platform:` value.
    - `docker/.env.example` documents `DOCKER_PLATFORM=` (empty by default), `IMAGE_TAG=dev`, `GHCR_OWNER=controleasy`, plus the comment block listing the three supported platforms and the rule "leave DOCKER_PLATFORM empty on native hosts".
    - On an Apple Silicon host with `DOCKER_PLATFORM=` unset, `docker compose config | grep platform` resolves to an empty string (or no `platform:` key) for every service.
    - On an Apple Silicon host with `DOCKER_PLATFORM=linux/arm64` set, `docker compose config | grep platform` resolves to `linux/arm64` for every service.
    - On a Linux x86_64 host with `DOCKER_PLATFORM=` unset, `docker compose up -d` pulls `linux/amd64` variants (no behavior change vs. the base spec).

- [ ] **1.21** Update the `Makefile` with `build-arm64`, `build-multi`, `inspect`, and `platform-check` targets.
  - **Amends:** base spec task **1.17** (Makefile).
  - **Acceptance criteria:**
    - `make build-arm64` invokes `docker buildx build --platform linux/arm64` for both the api and the web image, tagged with the `GHCR_OWNER` / `IMAGE_TAG` env vars (with the `:-dev` default).
    - `make build-multi` invokes `docker buildx bake -f docker/docker-bake.hcl` with `--set IMAGE_TAG=...` and `--set GHCR_OWNER=...`.
    - `make inspect IMAGE=<ref>` invokes `docker buildx imagetools inspect $(IMAGE)` and prints the multi-arch manifest to stdout.
    - `make platform-check` prints the host platform via `docker version --format '{{.Server.Os}}/{{.Server.Arch}}'`, prints the active `DOCKER_PLATFORM` value (or `<empty = host-native>`), and emits a warning (does **not** fail) when the two disagree.
    - `make help` (or the top of the Makefile) lists the new targets.

- [ ] **1.22** Add `/.github/workflows/build-multiarch.yml` with the build matrix, manifest, smoke, and signing jobs.
  - **Amends:** base spec Continuous task **C.1** (GitHub Actions `build` / `docker` / `smoke`).
  - **Acceptance criteria:**
    - Workflow file exists at `/.github/workflows/build-multiarch.yml`.
    - Job `build-api-linux` is a matrix `[ubuntu-latest, ubuntu-24.04-arm]`, runs `docker buildx build --platform linux/${{ matrix.arch }} --push` for the api image, uses `docker/setup-buildx-action` v3.
    - Job `build-web-linux` is the same matrix for the web image.
    - Job `build-api-windows-arm64` runs on `windows-11-arm` (with the documented fallback — see delta task 1.25), builds the api image for `windows/arm64`, and pushes it with the `-windows-arm64` suffix tag.
    - Job `build-web-windows-arm64` is the same for the web image.
    - Job `manifest` runs on `ubuntu-latest`, depends on the four build jobs, and invokes `docker buildx imagetools create --tag ghcr.io/${GHCR_OWNER}/api:${IMAGE_TAG} ghcr.io/${GHCR_OWNER}/api:tmp-amd64 ghcr.io/${GHCR_OWNER}/api:tmp-arm64` (and the same for the web image). The Windows image is **not** combined into this manifest.
    - Job `smoke` is a matrix `[ubuntu-latest (amd64), ubuntu-24.04-arm (arm64), windows-11-arm (windows-arm64)]`, depends on `manifest`, pulls the just-built image, runs `docker compose up -d`, runs `curl -k https://localhost/health` and `curl -k https://localhost/api/v1/residents` (per the base spec's Phase 1 verification gate), and tears the stack down with `docker compose down -v`. The `windows-11-arm` job uses `continue-on-error: true` and a clear "Windows runner unavailable" annotation if the runner is missing.
    - Job `sign` runs on `ubuntu-latest`, depends on `manifest`, and invokes `cosign sign --yes ghcr.io/${GHCR_OWNER}/api:${IMAGE_TAG}@$(docker buildx imagetools inspect --raw ghcr.io/${GHCR_OWNER}/api:${IMAGE_TAG} | jq -r '.manifests[] | select(.platform.architecture=="amd64") | .digest')` (and the same for `arm64` and for the web image). Authentication uses the GitHub OIDC token (`sigstore/cosign-installer` + `actions/attest`).
    - The workflow file uses `docker/setup-buildx-action` v3 and `docker/login-action` v3; no QEMU is used in any job.

- [ ] **1.23** Add `docs/architecture/decisions/0004-multi-arch-linux-and-windows-arm64.md`.
  - **Acceptance criteria:**
    - File exists at `docs/architecture/decisions/0004-multi-arch-linux-and-windows-arm64.md`.
    - Captures the six decisions listed in the `design.md` "ADR 0004" subsection: scope, base image choice, build strategy, CI runner choice, Windows on ARM runner availability caveat, Compose `DOCKER_PLATFORM` opt-in pattern.
    - Cites the base image manifest digests recorded at task 1.19 verification time.
    - The Phase 1 verification gate of the base spec is **explicitly amended** in the ADR's "Decision" section (in the delta, not in the base file — see delta task 1.25).

- [ ] **1.24** Add `docs/operations/arm64.md`.
  - **Acceptance criteria:**
    - File exists at `docs/operations/arm64.md`.
    - Lists the supported platforms (the matrix from `design.md`).
    - Documents the verification command `docker inspect --format '{{.Platform}}' <container>`.
    - Documents the wrong-arch pull diagnosis (the dev override no longer pins a platform, so the engine is the source of truth).
    - Includes an Apple Silicon quick-start, a Windows on ARM quick-start, and an AWS Graviton deployment note.
    - Documents the known image-tag pitfalls (any pinned minor version of `mysql:8.0`, `traefik:v3.1`, etc. must be one that has a `linux/arm64` layer).

- [ ] **1.25** Amend the base spec's Phase 1 verification gate (encoded as an addendum in this delta, not as an edit to the base file).
  - **Amends:** the "Verification gate (Phase 1)" block at the end of base spec `tasks.md` (the block that lists `docker compose up -d < 2 min`, `curl -k https://localhost/health`, etc.).
  - **Acceptance criteria:**
    - An addendum to the Phase 1 verification gate is added in `.specs/1 - modernization-roadmap-arm64/orchestration.md` (this delta) and in ADR 0004 (`docs/architecture/decisions/0004-multi-arch-linux-and-windows-arm64.md`), explicitly listing the three additional assertions:
      1. `docker buildx imagetools inspect ghcr.io/<org>/api:dev` lists `linux/amd64` and `linux/arm64`. The sibling tag `ghcr.io/<org>/api:dev-windows-arm64` points to a `windows/arm64` image. (And the same for `web`.)
      2. `/.github/workflows/build-multiarch.yml` exists and is green on the most recent run.
      3. The base spec's `docker compose up -d < 2 min` gate is re-verified separately on each of `ubuntu-latest` (amd64), `ubuntu-24.04-arm` (arm64), and `windows-11-arm` (arm64 Windows) — codified as three jobs in the new workflow.
    - If the `windows-11-arm` runner is **not** available to the repository, the third assertion is **explicitly marked as a known gap** with the documented fallback (manual-publish workflow + release note) and the `continue-on-error: true` annotation in the smoke job. The verification gate is still considered "green" with this gap explicitly documented, but the gap is recorded in `orchestration.md` and in the release checklist.
    - The base spec file `.specs/1 - modernization-roadmap/tasks.md` is **not modified** by this delta. The amendment is recorded only in the delta and in ADR 0004.

---

## Verification gate (Phase 1A)

- `docker buildx imagetools inspect ghcr.io/<org>/api:dev` lists `linux/amd64` and `linux/arm64`. The sibling tag `ghcr.io/<org>/api:dev-windows-arm64` points to a `windows/arm64` image.
- The same for `ghcr.io/<org>/web:dev`.
- `/.github/workflows/build-multiarch.yml` exists and is green on the most recent run.
- `make platform-check` on an Apple Silicon host with `DOCKER_PLATFORM=` unset prints `linux/arm64` and `<empty = host-native>`, with no warning.
- `make inspect IMAGE=ghcr.io/<org>/api:dev` prints the multi-arch manifest to stdout.
- The base spec's `docker compose up -d < 2 min` gate is re-verified separately on `ubuntu-latest` (amd64), `ubuntu-24.04-arm` (arm64), and `windows-11-arm` (windows/arm64) — codified as three jobs in the new workflow (or, for `windows-11-arm`, explicitly documented as a known gap with a fallback).
- `cosign verify --certificate-identity-regexp 'https://github.com/.*' --certificate-oidc-issuer 'https://token.actions.githubusercontent.com' ghcr.io/<org>/api:dev` succeeds.
- ADR 0004 is committed at `docs/architecture/decisions/0004-multi-arch-linux-and-windows-arm64.md`.
- `docs/operations/arm64.md` is committed and reviewed by an SRE who has never seen the workflow.

---

## Task Dependency Graph

The Task Dependency Graph below defines the parallel execution waves for Phase 1A. Tasks in the same wave have no dependencies on each other and can run in parallel. A wave only starts after all tasks in the previous wave are complete.

```json
{
  "waves": [
    { "wave": 1, "tasks": ["1.18", "1.19"] },
    { "wave": 2, "tasks": ["1.20", "1.21"] },
    { "wave": 3, "tasks": ["1.22", "1.23", "1.24"] },
    { "wave": 4, "tasks": ["1.25"] }
  ]
}
```

**Wave rationale:**

- **Wave 1** (1.18, 1.19): establish the build matrix (`docker-bake.hcl`) and the multi-arch-safe Dockerfiles. These are the foundation; every later task depends on them.
- **Wave 2** (1.20, 1.21): update the top-level Compose and the Makefile to consume the new build artifacts. Both tasks read from the Wave 1 outputs and are independent of each other.
- **Wave 3** (1.22, 1.23, 1.24): add the CI workflow, the ADR, and the operations page. The CI workflow depends on the Wave 2 outputs; the ADR and the operations page can be drafted in parallel and cite the Wave 1 / Wave 2 outputs.
- **Wave 4** (1.25): encode the Phase 1 verification gate amendment in `orchestration.md` and in ADR 0004. Depends on every other task in this delta, because the amendment must reference the actual outputs of 1.18-1.24.
