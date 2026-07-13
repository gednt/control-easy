# Design — Multi-Architecture Support (linux/amd64, linux/arm64, windows/arm64)

> Spec folder: `.specs/1 - modernization-roadmap-arm64/`
> **Delta spec.** Sibling of `.specs/1 - modernization-roadmap/` (the "base spec"). The base spec defines the single-platform shape of the Docker / Compose / CI stack. This delta extends that shape to support `linux/amd64`, `linux/arm64`, and `windows/arm64` as first-class targets, with native (non-emulated) builds and smoke tests on every supported platform.
>
> **Amends** (cross-references into the base spec — read in conjunction):
> - `design.md` § "Containerization" (the `docker-compose.yml` sketch, the `api.Dockerfile` sketch, the `mermaid flowchart LR`).
> - `tasks.md` Phase 1 — tasks 1.9, 1.10 and the Phase 1 verification gate; Continuous task C.1.
> - `design.md` § "Components & Files (Phase 1 deliverable)" — adds the bake file, the multi-arch CI workflow, the two new docs, and the Makefile targets.

## Overview

**ControlEasy Reborn** is being built and shipped as a containerized platform. The base spec defines a Docker / Compose / GitHub Actions pipeline that is implicitly **single-platform** (`linux/amd64`), which works on Intel/AMD servers, on Linux CI runners, and on Intel Macs but forces an emulated execution path on Apple Silicon Macs (`linux/amd64` container on an `arm64` Docker engine via QEMU), on AWS Graviton, and on Windows on ARM. Emulation works, but it is slow, it drains laptop batteries, it masks platform-specific breakages, and it forfeits the price/perf benefit that is the primary reason to deploy on Graviton in the first place.

This delta promotes `linux/amd64`, `linux/arm64`, and `windows/arm64` to **first-class supported platforms** of the platform. The api (`mcr.microsoft.com/dotnet/aspnet:8.0` runtime), the web static bundle (`nginx:1.27-alpine`), the MySQL 8 database (`mysql:8.0`), the Traefik reverse proxy (`traefik:v3.1`), the optional Redis cache (`redis:7-alpine`), and the Adminer DB UI (`adminer:4.8`) are all chosen because their upstream manifests already include a `linux/arm64` layer (and, where applicable, a `windows/arm64` layer). The CI matrix is rebuilt around **native** GitHub Actions runners (`ubuntu-latest` for `linux/amd64`, `ubuntu-24.04-arm` for `linux/arm64`, `windows-11-arm` for `windows/arm64` when the runner is available to the repository) so that build and smoke-test steps run without QEMU. Local development is changed so that the top-level `docker-compose.yml` does not pin a `platform:` value — the Docker engine picks the host-native variant automatically. The only way to opt into a non-native arch is the explicit `DOCKER_PLATFORM` environment variable, and the only way to combine the per-platform images into a single deployable tag is the explicit `docker buildx imagetools create` step in CI. Windows and Linux images cannot live in the same OCI image manifest, so the Windows image is published as a **sibling tag** (e.g., `api:tag` for the combined `linux/amd64`+`linux/arm64` manifest, and `api:tag-windows-arm64` for the `windows/arm64` image).

The companion `tasks.md` turns this into an executable checklist with verification gates. The Phase 1 verification gate of the base spec is **amended** (not edited) to add three new assertions: (a) `docker buildx imagetools inspect` on every published image lists the required platforms, (b) the multi-arch CI workflow is green on its most recent run, and (c) the native-runner smoke matrix is green for `linux/amd64`, `linux/arm64`, and `windows/arm64` (or the windows gap is explicitly documented as a known issue).

## Glossary

| Term | Meaning |
|---|---|
| **Multi-arch image** | An OCI image whose manifest lists more than one `platform` entry (`linux/amd64`, `linux/arm64`, ...). A single `docker pull` selects the right layer for the host's architecture. |
| **Multi-arch manifest** | The OCI image manifest (or manifest list) that ties together the per-platform images under a single tag. Built with `docker buildx build --platform ...` or composed with `docker buildx imagetools create`. |
| **`TARGETOS` / `TARGETARCH`** | Build args automatically set by `docker buildx` for every `FROM` in a multi-platform build. `TARGETARCH` is `amd64` or `arm64`; `TARGETOS` is `linux` or `windows`. Used inside a Dockerfile to gate platform-specific steps (e.g., `apt-get` vs. `choco`, or the choice of a `linux-musl-x64` vs. `linux-musl-arm64` .NET RID). |
| **`BUILDPLATFORM`** | The architecture of the **build** machine (the CI runner). With native-runner CI, `BUILDPLATFORM == TARGETPLATFORM`, so there is no QEMU translation. |
| **`docker buildx bake`** | Declarative multi-target multi-platform build, driven by an `hcl` file (`docker-bake.hcl`). Lets us express the `api` and `web` targets plus their `linux` and `windows` variants in one place. |
| **`docker buildx imagetools create`** | Composes an image manifest list from pre-built per-platform images. Used in the CI `manifest` job to combine the `linux/amd64` and `linux/arm64` builds into one tag. |
| **Native runner** | A GitHub Actions runner whose CPU architecture matches the target platform. `ubuntu-24.04-arm` is an arm64 Linux runner; `windows-11-arm` is an arm64 Windows runner (subject to availability — see Risks). |
| **`DOCKER_PLATFORM` opt-in** | The environment variable read by `docker-compose.yml` as `platform: ${DOCKER_PLATFORM:-}`. Empty by default; set explicitly to opt into a cross-arch build (e.g. `arm64` image on an `amd64` host). |
| **Sibling tag** | A second tag for the same service that carries the `windows/arm64` image, since Windows images cannot be combined with Linux images in a single OCI manifest. Convention: `<image>:<tag>` for Linux multi-arch, `<image>:<tag>-windows-arm64` for the Windows image. |
| **Cosign / GHCR attestations** | Image signing. The delta prescribes cosign keyless signing tied to the GitHub Actions OIDC token for every multi-arch manifest, with production deployments pinning the digest in the Compose / Helm values file. |
| **Wrong-arch pull** | A failure mode where a developer / CI runner pulls the `linux/amd64` variant on an `arm64` host (or vice versa). Detectable with `docker inspect --format '{{.Platform}}' <container>`; mitigated by the base spec's `docker-compose.yml` amendment (no top-level `platform:`) and by the new `make platform-check` target. |
| **Base image manifest digest** | The SHA256 digest of an upstream base image's manifest list (e.g., `mcr.microsoft.com/dotnet/aspnet:8.0` → `sha256:...`). Pinning the digest in the Dockerfile (with a renovate / dependabot follow-up) guarantees that a tag like `:8.0` cannot quietly lose a `linux/arm64` layer in a future upstream rebuild. |

## Architecture

### Platform Matrix (target state)

| Image | `linux/amd64` | `linux/arm64` | `windows/arm64` | Notes |
|---|---|---|---|---|
| `controleasy/api` | yes | yes | yes (sibling tag) | ASP.NET Core 8 on `mcr.microsoft.com/dotnet/aspnet:8.0`. |
| `controleasy/web` | yes | yes | yes (sibling tag) | Angular static bundle on `nginx:1.27-alpine`. |
| `controleasy/db` (`mysql:8.0`) | yes | yes | no | MySQL does not ship a `windows/*` container image; the dev `docker compose` path stays Linux-only. |
| `controleasy/reverse-proxy` (`traefik:v3.1`) | yes | yes | yes (sibling tag) | Optional in the Windows sibling manifest; production Windows on ARM hosts typically terminate TLS at the load balancer. |
| `controleasy/adminer` (`adminer:4.8`) | yes | yes | no | Dev-only. |
| `controleasy/redis` (`redis:7-alpine`, optional) | yes | yes | no | Dev-only. |

**Why `windows/arm64` cannot share a manifest with Linux images:** an OCI image manifest list is partitioned by **operating system**, not by architecture. `linux/amd64` and `linux/arm64` share a manifest; `windows/arm64` must be a separate manifest. We publish it as a sibling tag so deploy tooling can pick it with a single string (`<image>:<tag>-windows-arm64`).

### Base Image Choice (with manifest digests to verify at implementation time)

All base images below are already multi-arch manifests upstream. The implementation must (a) verify the manifest digest at task 1.19 time, (b) pin the digest in the Dockerfile (with the `:tag` retained for readability), and (c) document the digest in `docs/architecture/decisions/0004-multi-arch-linux-and-windows-arm64.md`.

| Role | Base image (build) | Base image (runtime) | Multi-arch status |
|---|---|---|---|
| API build | `mcr.microsoft.com/dotnet/sdk:8.0` | n/a | Multi-arch manifest includes `linux/amd64`, `linux/arm64`, `windows/amd64`, `windows/arm64`. |
| API runtime | n/a | `mcr.microsoft.com/dotnet/aspnet:8.0` | Same multi-arch manifest as above. |
| Web build | `node:20-bookworm-slim` | n/a | Multi-arch manifest includes `linux/amd64`, `linux/arm64` (and windows variants if a Windows build path is needed). |
| Web runtime | n/a | `nginx:1.27-alpine` | Multi-arch manifest includes `linux/amd64`, `linux/arm64`. |
| DB | n/a | `mysql:8.0` | Multi-arch manifest includes `linux/amd64`, `linux/arm64` as of `8.0.x`. Any minor version pin must be one that has a `linux/arm64` layer. |
| Reverse proxy | n/a | `traefik:v3.1` | Multi-arch manifest includes `linux/amd64`, `linux/arm64`, `windows/amd64`, `windows/arm64`. |
| Adminer | n/a | `adminer:4.8` | Multi-arch manifest includes `linux/amd64`, `linux/arm64`. |
| Redis (optional) | n/a | `redis:7-alpine` | Multi-arch manifest includes `linux/amd64`, `linux/arm64`. |

**Pin rule:** prefer the multi-arch-friendly tags (`-alpine`, `-slim`, `-bookworm`). Avoid tags that historically have been amd64-only. The verification step at task 1.19 calls `docker buildx imagetools inspect <base>:<tag>` and asserts that the manifest includes the required platforms before any application image is built.

### Local-Development Matrix

| Host platform | Engine picks | Result |
|---|---|---|
| Apple Silicon Mac (M1/M2/M3/M4) | `linux/arm64` | Native. No Rosetta. No QEMU. `docker compose up` pulls `linux/arm64` variants. |
| Windows on ARM (Snapdragon X) | `linux/arm64` (Linux containers via Docker Desktop's arm64 engine) or `windows/arm64` (Windows containers) | Native. No x64 emulation layer. |
| Linux x86_64 | `linux/amd64` | Same as before. No behavior change. |
| Linux aarch64 (e.g., Graviton dev box) | `linux/arm64` | Native. |
| Intel Mac (legacy) | `linux/amd64` | Same as before. No behavior change. |

The top-level `docker-compose.yml` does **not** set a top-level `platform:` value, so the Docker engine picks the host-native variant automatically. The dev override file (`docker-compose.override.yml`) also does not pin a platform. Cross-arch builds (e.g., an `amd64` developer wanting to test the `arm64` image) are an **explicit opt-in** via the `DOCKER_PLATFORM` environment variable.

### `docker-compose.yml` (amends base spec, sketch)

```yaml
# docker/docker-compose.yml
services:
  reverse-proxy:
    image: traefik:v3.1
    platform: ${DOCKER_PLATFORM:-}   # empty = host-native; opt in by setting DOCKER_PLATFORM
    command: --providers.file.directory=/etc/traefik/dynamic
    ports: ["80:80", "443:443"]
    volumes:
      - ./reverse-proxy/traefik.yml:/etc/traefik/static.yml:ro
      - ./reverse-proxy/dynamic.yml:/etc/traefik/dynamic/dynamic.yml:ro
      - traefik-data:/letsencrypt

  api:
    image: ghcr.io/${GHCR_OWNER:-controleasy}/api:${IMAGE_TAG:-dev}
    platform: ${DOCKER_PLATFORM:-}
    build:
      context: ..
      dockerfile: docker/api.Dockerfile
      platforms:
        - linux/amd64
        - linux/arm64
    # (only when building the windows variant; see docker-bake.hcl)
    # platforms:
    #   - windows/arm64
    environment:
      Db__Provider: MySQL
      Db__Host: db
      # ... unchanged from the base spec ...
    depends_on: { db: { condition: service_healthy } }

  web:
    image: ghcr.io/${GHCR_OWNER:-controleasy}/web:${IMAGE_TAG:-dev}
    platform: ${DOCKER_PLATFORM:-}
    build:
      context: ..
      dockerfile: docker/web.Dockerfile
      platforms:
        - linux/amd64
        - linux/arm64

  db:
    image: mysql:8.0
    platform: ${DOCKER_PLATFORM:-}
    # ... unchanged from the base spec ...

  adminer:
    image: adminer:4.8
    platform: ${DOCKER_PLATFORM:-}
    # ... unchanged from the base spec ...

  redis:                          # optional, dev only
    image: redis:7-alpine
    platform: ${DOCKER_PLATFORM:-}

volumes:
  mysql-data:
  traefik-data:
```

The `platform: ${DOCKER_PLATFORM:-}` line is the **single** mechanism that injects a non-native platform. With the variable empty (the default), Docker Engine picks the host-native variant and the variable disappears from the resolved Compose config.

### Dockerfile Amendments (amends base spec tasks 1.9)

```dockerfile
# docker/api.Dockerfile (delta amendments only)
# --- build stage ---
FROM --platform=$BUILDPLATFORM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY Directory.Packages.props Directory.Build.props ./
COPY src/ ./src/
# Default to the host arch for restore/publish when no explicit RID is needed;
# multi-arch safe because the .NET 8 SDK image carries the linux/arm64 toolchain.
ARG TARGETARCH
ARG TARGETOS
RUN dotnet restore src/Host/ControlEasyReborn.Api/ControlEasyReborn.Api.csproj \
    -a $TARGETARCH
RUN dotnet publish src/Host/ControlEasyReborn.Api/ControlEasyReborn.Api.csproj \
    -c Release -o /app /p:UseAppHost=false -a $TARGETARCH

# --- runtime stage ---
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
COPY --from=build /app .
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080
ENTRYPOINT ["dotnet", "ControlEasyReborn.Api.dll"]
```

```dockerfile
# docker/web.Dockerfile (delta amendments only)
# --- build stage: build the Angular bundle ---
FROM --platform=$BUILDPLATFORM node:20-bookworm-slim AS build
WORKDIR /src
COPY src/Web/ControlEasyReborn.Web/package*.json ./
RUN npm ci
COPY src/Web/ControlEasyReborn.Web/ ./
ARG TARGETARCH
# Angular 18 has no native build step, so TARGETARCH is informational here;
# retained for symmetry with the api image and in case a future native
# dependency (e.g. sharp) is added.
RUN npm run build

# --- runtime stage: serve the bundle from nginx ---
FROM nginx:1.27-alpine AS runtime
COPY --from=build /src/dist/ControlEasyReborn.Web/browser /usr/share/nginx/html
COPY docker/reverse-proxy/nginx.conf /etc/nginx/conf.d/default.conf
EXPOSE 8080
CMD ["nginx", "-g", "daemon off;"]
```

The `FROM --platform=$BUILDPLATFORM` line on the build stage is the only change that matters for build speed and correctness on the `ubuntu-24.04-arm` runner. The runtime stage is left un-pinned so the buildx manifest list picks the matching `linux/arm64` or `linux/amd64` `aspnet:8.0` / `nginx:1.27-alpine` layer automatically.

### `docker/docker-bake.hcl` (new file, task 1.18)

```hcl
# docker/docker-bake.hcl
# Multi-arch build matrix for the api and web images.
# Linux: combined linux/amd64 + linux/arm64 manifest under the main tag.
# Windows: published as a sibling tag (see "sibling tag" in the glossary).

variable "IMAGE_TAG" {
  default = "dev"
}

variable "GHCR_OWNER" {
  default = "controleasy"
}

# Shared settings.
target "_common" {
  context = ".."
  platforms = ["linux/amd64", "linux/arm64"]
  output   = ["type=image,push=true"]
}

# Linux api + web.
target "api" {
  inherits = ["_common"]
  dockerfile = "docker/api.Dockerfile"
  tags = [
    "ghcr.io/${GHCR_OWNER}/api:${IMAGE_TAG}",
  ]
}

target "web" {
  inherits = ["_common"]
  dockerfile = "docker/web.Dockerfile"
  tags = [
    "ghcr.io/${GHCR_OWNER}/web:${IMAGE_TAG}",
  ]
}

# Windows on ARM api + web (sibling tag).
target "api-windows-arm64" {
  context = ".."
  dockerfile = "docker/api.Dockerfile"
  platforms = ["windows/arm64"]
  output    = ["type=image,push=true"]
  tags = [
    "ghcr.io/${GHCR_OWNER}/api:${IMAGE_TAG}-windows-arm64",
  ]
}

target "web-windows-arm64" {
  context = ".."
  dockerfile = "docker/web.Dockerfile"
  platforms = ["windows/arm64"]
  output    = ["type=image,push=true"]
  tags = [
    "ghcr.io/${GHCR_OWNER}/web:${IMAGE_TAG}-windows-arm64",
  ]
}

# Convenience: build everything in one command.
group "default" {
  targets = ["api", "web", "api-windows-arm64", "web-windows-arm64"]
}
```

### CI Strategy (GitHub Actions, native runners)

The base spec's C.1 job `docker` (build images, push to GHCR) and `smoke` (docker compose up + curl + Playwright) are **amended** by a new workflow file, `/.github/workflows/build-multiarch.yml`, that:

1. Builds `api` and `web` for `linux/amd64` and `linux/arm64` on `ubuntu-latest` and `ubuntu-24.04-arm` respectively (two jobs in a matrix).
2. Builds `api` and `web` for `windows/arm64` on `windows-11-arm` (or the documented fallback — see Risks) and pushes them with the `-windows-arm64` suffix tag.
3. Runs a `manifest` job that composes the two Linux images into a single manifest list under the main tag, and (separately) leaves the Windows image under the sibling tag.
4. Runs a `smoke` matrix that pulls the freshly built image on each of the three native runners, runs `docker compose up -d`, runs the base spec's `curl /health` and `curl /api/v1/residents` checks, and tears the stack down with `docker compose down -v`.
5. Signs every published manifest with cosign (keyless, tied to the GitHub OIDC token) and uploads the attestation to the GHCR image page.

All build jobs use `docker/setup-buildx-action` v3. The `linux/amd64` and `linux/arm64` builds produce the canonical multi-arch manifest; the `windows/arm64` build produces a sibling-tag image. The base spec's C.1 `docker` and `smoke` jobs can be **kept as the `linux/amd64` job** in the new matrix (no duplication of work) and the existing `win-x64` axis is removed in favor of `windows-11-arm` (which covers `windows/arm64`; `windows/amd64` is no longer a target — see Risks).

### `.env.example` additions (amends base spec)

```bash
# docker/.env.example (delta additions only)

# DOCKER_PLATFORM is empty by default, which makes docker compose pick the
# host-native variant (linux/amd64 on Intel/AMD, linux/arm64 on Apple Silicon
# and on Windows on ARM). Set it explicitly to opt into a cross-arch build,
# e.g. DOCKER_PLATFORM=linux/arm64 on an amd64 host that needs to test the
# arm64 image. Supported values: linux/amd64, linux/arm64, windows/arm64.
DOCKER_PLATFORM=

# IMAGE_TAG is the tag the api and web images are pushed under. CI sets it
# from the git ref (e.g. "sha-abc1234" for branches, "v1.2.3" for tags).
IMAGE_TAG=dev

# GHCR_OWNER is the GitHub organisation (or user) that owns the package
# registry where the multi-arch manifests are published.
GHCR_OWNER=controleasy

# Supported platforms (informational, surfaced by `make platform-check`):
#   - linux/amd64   (x86_64 servers, Intel/AMD desktops, Windows + WSL2 on Intel)
#   - linux/arm64   (Apple Silicon M1/M2/M3/M4, AWS Graviton 2/3/4,
#                    GCP Tau T2A, Azure Dpsv5, Raspberry Pi 5 64-bit,
#                    Linux on Snapdragon X)
#   - windows/arm64 (Windows on ARM: Surface Pro X, Surface Pro 11,
#                    Snapdragon X Elite / Plus laptops)
# Rule: leave DOCKER_PLATFORM empty on native hosts.
```

### Makefile additions (amends base spec task 1.17)

```makefile
# Makefile (delta additions only)

# Build the api and web images for linux/arm64 explicitly (one arch, one
# build, no QEMU on Apple Silicon). Useful when an arm64 developer wants
# to publish a single-arch dev tag without going through bake.
build-arm64:
	docker buildx build --platform linux/arm64 \
	  -t ghcr.io/$${GHCR_OWNER:-controleasy}/api:$${IMAGE_TAG:-dev} \
	  -f docker/api.Dockerfile ..
	docker buildx build --platform linux/arm64 \
	  -t ghcr.io/$${GHCR_OWNER:-controleasy}/web:$${IMAGE_TAG:-dev} \
	  -f docker/web.Dockerfile ..

# Build everything via the bake file (Linux + Windows).
build-multi:
	docker buildx bake -f docker/docker-bake.hcl \
	  --set IMAGE_TAG=$${IMAGE_TAG:-dev} \
	  --set GHCR_OWNER=$${GHCR_OWNER:-controleasy}

# Inspect the multi-arch manifest of an image. Pass IMAGE=...
# (e.g. `make inspect IMAGE=ghcr.io/controleasy/api:dev`).
inspect:
	docker buildx imagetools inspect $(IMAGE)

# Print the host platform, the active DOCKER_PLATFORM, and warn (but do not
# fail) if they disagree. Failure is expected for explicit cross-arch dev.
platform-check:
	@echo "Host:        $$(docker version --format '{{.Server.Os}}/{{.Server.Arch}}')"
	@echo "DOCKER_PLATFORM: $${DOCKER_PLATFORM:-<empty = host-native>}"
	@if [ "$${DOCKER_PLATFORM:-}" != "" ]; then \
	  echo "Warning: DOCKER_PLATFORM is set; Docker will pull a non-native arch."; \
	fi
```

### ADR 0004 — `docs/architecture/decisions/0004-multi-arch-linux-and-windows-arm64.md`

The new ADR (task 1.23) records, in the standard ADR shape, the six decisions encoded in this delta:

1. **Scope.** Supported platforms are `linux/amd64`, `linux/arm64`, `windows/arm64`. `linux/arm/v7` is explicitly excluded. Status and consequences are recorded in the ADR.
2. **Base image choice.** The six upstream base images listed in the "Base Image Choice" table above, with the multi-arch manifest digests cited at implementation time.
3. **Build strategy.** `docker buildx bake` driven by `docker/docker-bake.hcl`; per-platform images pushed by CI; a `manifest` job composes the Linux multi-arch manifest under the main tag and leaves the Windows image as a sibling tag.
4. **CI runner choice.** `ubuntu-latest` for `linux/amd64`, `ubuntu-24.04-arm` for `linux/arm64`, `windows-11-arm` for `windows/arm64` (or documented fallback). No QEMU.
5. **Windows on ARM runner availability caveat.** Recorded in the ADR's "Status" section; if `windows-11-arm` is not available to the repository, the Windows build is performed via a manual publish workflow and the gap is called out in the release notes.
6. **Compose `DOCKER_PLATFORM` opt-in pattern.** `platform: ${DOCKER_PLATFORM:-}` in the top-level `docker-compose.yml`, empty by default, opt-in for cross-arch dev. The dev override does not pin a platform.

The ADR also amends the base spec's Phase 1 verification gate (encoded as an **addendum in the delta**, not as an edit to the base file — see delta task 1.25):

- `docker buildx imagetools inspect ghcr.io/<org>/api:dev` must list `linux/amd64`, `linux/arm64` (and the `-windows-arm64` sibling tag must point to a `windows/arm64` image).
- The same for `ghcr.io/<org>/web:dev`.
- `/.github/workflows/build-multiarch.yml` exists and is green on the most recent run.
- The base spec's `docker compose up -d < 2 min` gate is re-verified separately on `ubuntu-latest` (amd64), `ubuntu-24.04-arm` (arm64), and `windows-11-arm` (arm64 Windows) — codified as three jobs.

### `docs/operations/arm64.md` (new file, task 1.24)

The operations page covers:

- **Supported platforms** (the matrix above, copy-pasted).
- **How to verify the running image's platform:** `docker inspect --format '{{.Platform}}' <container>` should return `linux/amd64` or `linux/arm64` (or `windows/arm64` for the Windows sibling image). If it returns the wrong value, the host has pulled the wrong-arch variant.
- **How to diagnose a wrong-arch pull:** the dev override file no longer pins a platform, so the engine is the source of truth. If the engine is `arm64` and the container is `linux/amd64`, either the image tag has no `linux/arm64` layer (rebuild with `make build-arm64` and re-push) or the `DOCKER_PLATFORM` env var is leaking from the shell.
- **Apple Silicon quick-start:** nothing to do — `docker compose up` is native.
- **Windows on ARM quick-start:** install Docker Desktop for Windows on ARM and run `docker compose up` from WSL2 or PowerShell; the engine picks `linux/arm64` automatically.
- **AWS Graviton deployment note:** use the `linux/arm64` variant of the api and web image; ECS / Fargate / EKS all select the right layer automatically from the multi-arch manifest. If a wrong-arch pull is observed, verify that the image manifest includes `linux/arm64` with `docker buildx imagetools inspect <image>:<tag>`.
- **Known image-tag pitfalls:** any pinned minor version of `mysql:8.0`, `traefik:v3.1`, etc. must be one that has a `linux/arm64` layer; CI fails the build if `docker buildx imagetools inspect` returns a single-platform manifest for any base image.

## Success Criteria

1. `docker compose up -d` on an Apple Silicon Mac pulls `linux/arm64` variants of `api`, `web`, `db`, `reverse-proxy`, `adminer`, `redis`. Verified with `docker inspect --format '{{.Platform}}' <container>` returning `linux/arm64` for every service.
2. `docker compose up -d` on a Windows on ARM host pulls `linux/arm64` variants. Same verification command.
3. `docker compose up -d` on a Linux x86_64 host pulls `linux/amd64` variants (no behavior change vs. the base spec).
4. `make inspect IMAGE=ghcr.io/<org>/api:dev` shows a multi-arch manifest with at least `linux/amd64` and `linux/arm64`. The sibling tag `ghcr.io/<org>/api:dev-windows-arm64` exists and is a `windows/arm64` image.
5. `make platform-check` prints the host platform, the active `DOCKER_PLATFORM` value, and warns (does not fail) when they disagree.
6. CI on `ubuntu-latest`, `ubuntu-24.04-arm`, and `windows-11-arm` is green for the build, manifest, and smoke jobs.
7. Every published manifest is cosign-signed; `docker buildx imagetools inspect --raw <image>:<tag> | jq '.manifests[] | select(.platform.architecture=="amd64")'` returns a digest that matches the cosign attestation in the GHCR package page.
8. The Phase 1 verification gate of the base spec is amended (addendum in this delta) to add the three multi-arch assertions listed in the "ADR 0004" subsection.

## Components & Files (Phase 1A deliverable)

| Path | Purpose | Task |
|---|---|---|
| `docker/docker-bake.hcl` | Multi-arch build matrix (Linux + Windows sibling target). | 1.18 |
| `docker/api.Dockerfile` | Multi-arch safe: `FROM --platform=$BUILDPLATFORM sdk:8.0`, runtime on multi-arch `aspnet:8.0`. | 1.19 (amends base 1.9) |
| `docker/web.Dockerfile` | Build on multi-arch `node:20-bookworm-slim`, runtime on multi-arch `nginx:1.27-alpine`. | 1.19 (amends base 1.9) |
| `docker/docker-compose.yml` | Adds `platform: ${DOCKER_PLATFORM:-}` to every service; no top-level `platform:` pin. | 1.20 (amends base 1.10) |
| `docker/.env.example` | Adds `DOCKER_PLATFORM=`, `IMAGE_TAG=`, `GHCR_OWNER=`, and the supported-platforms comment block. | 1.20 (amends base 1.10) |
| `Makefile` | Adds `build-arm64`, `build-multi`, `inspect`, `platform-check` targets. | 1.21 (amends base 1.17) |
| `/.github/workflows/build-multiarch.yml` | CI matrix: `linux/amd64`, `linux/arm64`, `windows/arm64`; manifest job; smoke job; cosign signing. | 1.22 (amends base C.1) |
| `docs/architecture/decisions/0004-multi-arch-linux-and-windows-arm64.md` | ADR recording the six decisions. | 1.23 |
| `docs/operations/arm64.md` | Operations page: supported platforms, verification, troubleshooting, Graviton / Apple Silicon / Windows on ARM quick-start. | 1.24 |
| `.specs/1 - modernization-roadmap-arm64/requirements.md` | This delta's user stories (UC-26..UC-33). | n/a |
| `.specs/1 - modernization-roadmap-arm64/design.md` | This file. | n/a |
| `.specs/1 - modernization-roadmap-arm64/tasks.md` | This delta's tasks. | n/a |
| `.specs/1 - modernization-roadmap-arm64/orchestration.md` | This delta's orchestration log. | n/a |

## Verification Approach

- **Per task:** each task ends with a verification command (`docker buildx imagetools inspect`, `make inspect`, a `docker compose up -d` + curl smoke test, a CI workflow file path check).
- **Per phase:** the Phase 1A gate (delta task 1.25) is fully green before the delta is considered done.
- **Continuous:** the new `build-multiarch.yml` workflow runs on every PR; the `linux/amd64` and `linux/arm64` jobs must be green for the PR to be mergeable. The `windows/arm64` job is **strongly recommended** but is a documented gap if the runner is unavailable (see Risks).
- **Visual:** the `docs/operations/arm64.md` page is reviewed by an SRE who has never seen the workflow before, to confirm the page is sufficient to deploy on Graviton / Apple Silicon / Windows on ARM.

## Risks & Mitigations

| Risk | Mitigation |
|---|---|
| `windows-11-arm` GitHub-hosted runner is not available to the repository (the runner is in public preview / limited rollout as of the date of this delta). | Documented in the ADR and in `orchestration.md`. The `windows/arm64` build is performed via a documented manual-publish workflow (or skipped with a release note). The `linux/amd64` + `linux/arm64` matrix is still green. The smoke gate for `windows/arm64` is a `continue-on-error: true` step with a clear "Windows runner unavailable" annotation. |
| An upstream base image (e.g., a future `mysql:8.x.y`) silently drops its `linux/arm64` layer. | Task 1.19 includes a CI step that runs `docker buildx imagetools inspect` on every base image and asserts that the manifest includes the required platforms before the application image is built. A pin-to-digest follow-up is filed as a separate ADR. |
| `mcr.microsoft.com/dotnet/aspnet:8.0` upstream rebases and the `linux/arm64` layer changes underneath us. | Same as above. Pin the digest in the Dockerfile (task 1.19) and add a Renovate / Dependabot rule to bump it intentionally. |
| A developer on Apple Silicon forgets to remove `DOCKER_DEFAULT_PLATFORM=linux/amd64` from their shell rc and silently runs an emulated stack. | `make platform-check` warns (does not fail) when `DOCKER_PLATFORM` is set or when the host arch and the active override disagree. The top-level `docker-compose.yml` is changed so the variable is the **only** opt-in; there is no `DOCKER_DEFAULT_PLATFORM` reference in the repo. |
| An attacker pushes a single-platform image to a tag that previously carried a multi-arch manifest, swapping an `amd64` image into an `arm64` production host. | Cosign keyless signing tied to the GitHub OIDC token; production deployments pin the digest in the Compose / Helm values file. Verified by the `docker buildx imagetools inspect --raw` + cosign attestation cross-check. |
| `linux/arm/v7` (32-bit ARM) creeps back into a downstream image tag (e.g. someone adds a `node:14-stretch` reference). | The base image pin rule in this delta explicitly prefers multi-arch-friendly tags (`-alpine`, `-slim`, `-bookworm`) and the CI base-image inspection step fails the build if a base image is single-platform. |
| MySQL 8's arm64 layer has, historically, lagged behind the amd64 layer by a minor version. | The Phase 1A gate asserts that the chosen `mysql:8.0.x` tag has a `linux/arm64` layer. The implementation (task 1.19) selects a tag that ships both layers. |
| The base spec's C.1 `windows-x64` matrix entry is removed by this delta (it is replaced by `windows-11-arm`). If the `windows-11-arm` runner is not yet available, the Windows CI coverage regresses. | The `linux/amd64` + `linux/arm64` CI is the primary coverage. The `windows/arm64` gap is explicit in the ADR and the orchestration log. Restoring `windows-x64` (the previous base-spec axis) is a one-line job addition if needed before `windows-11-arm` GA. |
