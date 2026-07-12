# Orchestration Log — Multi-Architecture Support (linux/amd64, linux/arm64, windows/arm64)

> Spec folder: `.specs/1 - modernization-roadmap-arm64/`
> One-page log for the multi-arch delta. Sibling of the base spec's `orchestration.md` at `.specs/1 - modernization-roadmap/orchestration.md`. This log is intentionally short: the base plan + delta are recorded below, the cross-references into the base spec are listed, and the residual risks (Windows-on-ARM runner availability, base-image manifest drift, EmguCV removal as a separate future spec) are recorded at the bottom.

## Base plan (recap from `.specs/1 - modernization-roadmap/`)

The base spec targets a single-platform (`linux/amd64`) Docker / Compose / GitHub Actions pipeline for the ControlEasy Reborn modernization: a `docker-compose.yml` with `api`, `web`, `db`, `reverse-proxy`, `adminer`, `seq` (Continuous C.1's `docker` job builds and pushes the api and web images to GHCR; `smoke` runs `docker compose up -d` + curl). The base spec's Phase 1 verification gate asserts that `docker compose up -d` brings all services healthy in under 2 minutes and that `curl -k https://localhost/api/v1/residents` returns 200 with `[]`.

## Delta scope (this spec)

Extend the base plan so the api and web images, the docker-compose stack, the local-dev Makefile, and the GitHub Actions CI all support **three platforms** (`linux/amd64`, `linux/arm64`, `windows/arm64`) on **native** runners, with cosign-signed multi-arch manifests pinned by digest in production. Local dev runs natively on Apple Silicon (no QEMU) and on Windows on ARM (no x64 emulation). MySQL 8 is pulled at `linux/arm64` natively on arm64 hosts.

## Decisions confirmed with the user (encoded verbatim from the task brief)

1. **Target platforms (option 1a from the brief).** `linux/amd64`, `linux/arm64`, `windows/arm64`. `linux/arm/v7` is **excluded** for v1.
2. **CI strategy (option 2a from the brief).** Native-hosted runners — `ubuntu-latest` for `linux/amd64`, `ubuntu-24.04-arm` for `linux/arm64`, `windows-11-arm` for `windows/arm64` (with the documented fallback below). No QEMU in the recommended path.
3. **Local dev.** Apple Silicon Mac and Windows on ARM run natively. No Rosetta, no `--platform linux/amd64` emulation, no `DOCKER_DEFAULT_PLATFORM` override. Linux x86_64 runs as before. The dev-time variant (`docker-compose.override.yml`) does not hard-pin a platform.
4. **Sibling delta spec.** The multi-arch work is a **sibling delta spec** (this folder), not a modification of the base spec. The base spec files in `.specs/1 - modernization-roadmap/` are **not modified** by this delegation. The Phase 1 verification gate amendment is encoded in this delta's `tasks.md` (task 1.25), in this `orchestration.md`, and in ADR 0004.

## Base-spec items amended (cross-references, not edits)

| Base spec item | Where it lives | Delta task that amends it |
|---|---|---|
| UC-9 (JWT auth, updated) | `requirements.md` | The multi-arch delta does not amend the JWT shape itself; it amends the deployment surface (multi-arch images) that UC-9's containerized API runs on. |
| UC-14 (docker-compose stack) | `requirements.md` | UC-14 is amended by delta UC-26 / UC-27 (native arm64 execution on Apple Silicon and on Windows on ARM) and by delta task 1.20. |
| UC-15 (multi-stage Dockerfiles) | `requirements.md` | UC-15 is amended by delta UC-28 / UC-29 / UC-31 and by delta task 1.19. |
| UC-16 (secrets injection) | `requirements.md` | UC-16 is amended by the base image choice in delta design (`mcr.microsoft.com/dotnet/aspnet:8.0` etc. are all multi-arch manifests, so secrets injection stays platform-agnostic). |
| Phase 1 task 1.9 (Dockerfiles) | `tasks.md` | Amended by delta task 1.19. |
| Phase 1 task 1.10 (`docker-compose.yml`) | `tasks.md` | Amended by delta task 1.20. |
| Phase 1 task 1.17 (Makefile) | `tasks.md` | Amended by delta task 1.21. |
| Continuous task C.1 (GitHub Actions `build` / `docker` / `smoke`) | `tasks.md` | Amended by delta task 1.22. |
| Phase 1 verification gate | `tasks.md` | Amended (addendum in the delta) by delta task 1.25. |
| Components & Files table | `design.md` | Amended by the new table in this delta's `design.md` (Phase 1A deliverable). |
| Containerization subsection | `design.md` | Amended by the new Docker / Compose / Bake / CI subsections in this delta's `design.md`. |

## Chosen base images (with multi-arch status, recorded verbatim from the task brief)

| Role | Base image | Multi-arch status |
|---|---|---|
| API build | `mcr.microsoft.com/dotnet/sdk:8.0` | Multi-arch (linux/amd64, linux/arm64, windows/amd64, windows/arm64). |
| API runtime | `mcr.microsoft.com/dotnet/aspnet:8.0` | Multi-arch (same). |
| Web build | `node:20-bookworm-slim` | Multi-arch (linux/amd64, linux/arm64; windows variants if a Windows build path is needed). |
| Web runtime | `nginx:1.27-alpine` | Multi-arch (linux/amd64, linux/arm64). |
| DB | `mysql:8.0` | Multi-arch (linux/amd64, linux/arm64 as of `8.0.x`; any minor pin must include a `linux/arm64` layer). |
| Reverse proxy | `traefik:v3.1` | Multi-arch. |
| Adminer | `adminer:4.8` | Multi-arch. |
| Redis (optional) | `redis:7-alpine` | Multi-arch. |

All digests must be verified at task 1.19 implementation time and cited in ADR 0004.

## Cross-references to base spec items that the delta amends

- `requirements.md` UC-9, UC-14, UC-15, UC-16 — extended by delta UC-26..UC-33.
- `design.md` "Containerization" subsection — extended by the new Docker / Compose / Bake / CI subsections in this delta's `design.md`.
- `tasks.md` Phase 1 — tasks 1.9, 1.10, 1.17; the Phase 1 verification gate; Continuous task C.1 — see delta tasks 1.19, 1.20, 1.21, 1.22, 1.25.
- `design.md` "Components & Files (Phase 1 deliverable)" — extended by the new "Components & Files (Phase 1A deliverable)" table in this delta's `design.md`.

## Residual risks (recorded from the task brief, plus implementation-time risks surfaced by the design)

1. **`windows-11-arm` GitHub-hosted runner availability.** The runner exists in some form (public preview or limited rollout as of the date of this delta) but may not be available to every repository. The delta documents this as a known gap: the `windows/arm64` build is performed via a documented manual-publish workflow (or skipped with a release note), the `linux/amd64` + `linux/arm64` matrix is still green, and the smoke gate for `windows/arm64` is `continue-on-error: true` with a clear annotation. The base spec's previous C.1 axis `windows-x64` is **not** restored by this delta; if a `windows/amd64` smoke test is needed before `windows-11-arm` GA, it is a one-line job addition (out of scope for this delta).

2. **Base-image manifest drift.** An upstream base image (e.g., a future `mysql:8.x.y`) could silently drop its `linux/arm64` layer. Delta task 1.19's CI step asserts that every base image's manifest includes the required platforms before the application image is built. A pin-to-digest follow-up is filed as a separate ADR (not part of this delta).

3. **EmguCV removal is a separate future spec.** Inherited from the base spec's "Out of Scope" list. The multi-arch delta does not address the camera / facial-recognition integration.

4. **`linux/arm/v7` creeping back.** Mitigated by the base image pin rule (multi-arch-friendly tags only) and by the CI base-image inspection step in task 1.19. If a downstream image tag slips in, the build fails.

5. **Developer shell-leak of `DOCKER_DEFAULT_PLATFORM=linux/amd64` on Apple Silicon.** Mitigated by `make platform-check` and by the fact that the top-level `docker-compose.yml` is the only place that injects a non-native platform — there is no `DOCKER_DEFAULT_PLATFORM` reference in the repo.

6. **Multi-arch manifest swap attack.** Mitigated by cosign keyless signing tied to the GitHub OIDC token and by production deployments pinning the digest in the Compose / Helm values file.

7. **MySQL 8 arm64-layer lag.** Historically MySQL's `linux/arm64` layer has lagged behind `linux/amd64` by a minor version. The Phase 1A gate asserts that the chosen `mysql:8.0.x` tag has a `linux/arm64` layer; the implementation (task 1.19) selects a tag that ships both.

8. **C.1 `windows-x64` axis removed.** The base spec's C.1 matrix includes `win-x64`. This delta removes that axis in favor of `windows-11-arm`. If `windows-11-arm` is not yet GA when the workflow ships, the Windows CI coverage is reduced to "Windows image is built manually and published with a release note" until the runner is available. The `linux/amd64` + `linux/arm64` coverage is unaffected.
