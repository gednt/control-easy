# Requirements — Multi-Architecture Support (linux/amd64, linux/arm64, windows/arm64)

> Spec folder: `.specs/1 - modernization-roadmap-arm64/`
> **Delta spec.** Sibling of `.specs/1 - modernization-roadmap/` (the "base spec"). This document does **not** modify the base spec; it encodes the multi-architecture additions as user stories, design, and tasks that *amend* the base spec at well-defined cross-reference points.
>
> **Amends** (cross-references into the base spec — read in conjunction):
> - **UC-9 (updated)** in the base spec — same JWT auth, but every published container image must now exist for all target platforms.
> - **UC-14** — `docker-compose.yml` (local dev = production parity) now also requires native arm64 execution on Apple Silicon and on Windows on ARM.
> - **UC-15** — multi-stage Dockerfiles now also need to be multi-arch safe.
> - **UC-16** — secrets injection now also requires platform-agnostic base images (no amd64-only layers).
> - Base spec Phase 1 task **1.9** (multi-stage Dockerfiles) — see delta task 1.19.
> - Base spec Phase 1 task **1.10** (`docker-compose.yml`) — see delta task 1.20.
> - Base spec Continuous task **C.1** (GitHub Actions `build` / `docker` / `smoke` jobs) — see delta task 1.22 and the Phase 1 verification gate amendment in delta task 1.25.

## User Stories

### Platform Coverage

- **UC-26:** As a *developer on Apple Silicon (M1/M2/M3/M4)*, I want `docker compose up` to pull and run `linux/arm64` variants of every image natively (api, web, db, reverse-proxy, adminer, redis) so I do not need Rosetta, do not need QEMU, do not need `DOCKER_DEFAULT_PLATFORM=linux/amd64`, and the stack runs at full speed on my M-series Mac with full battery life and no JIT translation overhead.
- **UC-27:** As a *developer on Windows on ARM* (Surface Pro X, Surface Pro 11, Snapdragon X Elite / Plus laptops), I want the same native `linux/arm64` (or `windows/arm64` for Windows containers) experience with no x64 emulation layer, so the dev loop is fast, the Docker engine does not silently translate instructions, and the container platform matches the host platform.
- **UC-28:** As a *DevOps engineer*, I want a single multi-arch manifest for the `api` and `web` images that lists both `linux/amd64` and `linux/arm64` (with `windows/arm64` published as a sibling tag, because Windows images cannot be combined with Linux images in a single OCI manifest) so that the same tag is deployable to x86 servers, AWS Graviton, Apple Silicon hosts, and Windows on ARM hosts without per-platform rebuilds and without "wrong arch" image swaps at deploy time.
- **UC-29:** As a *DevOps engineer*, I want CI to build and smoke-test every supported platform on a **native** GitHub Actions runner (`ubuntu-latest` for `linux/amd64`, `ubuntu-24.04-arm` for `linux/arm64`, `windows-11-arm` for `windows/arm64` when available) so that we do not pay the QEMU emulation cost, and so that we catch platform-specific breakages (missing native binaries, wrong base image tag, missing arm64 layer on an upstream image) before the image is published.
- **UC-30:** As a *security officer*, I want every multi-arch manifest to be signed (cosign / GHCR attestations) and pinned by digest in production so that an attacker cannot swap an `amd64` image into an `arm64` production host (or vice versa) by pushing a single-platform image with the same tag, and so that a deploy cannot accidentally fall back to a wrong-arch variant when the host platform changes.
- **UC-31:** As an *SRE deploying to AWS Graviton 2/3/4* (and equivalent arm64 cloud hosts: GCP Tau T2A, Azure Dpsv5, Oracle Ampere A1), I want the `api` and `web` images to be `linux/arm64`-native with no x86 emulation inside the container, so we get the published Graviton price/perf benefit (typically 40% better cost per request versus the equivalent x86 instance) and we do not silently regress to QEMU when a developer forgets to pull the right arch.
- **UC-32:** As a *developer*, I want a `make build-arm64` / `make inspect` / `make platform-check` workflow and a `DOCKER_PLATFORM` opt-in environment variable (default empty → host-native) so I can opt into cross-arch builds (e.g. an `arm64` image on an `amd64` host) **explicitly**, never by accident, and so I can verify the manifest of any image I just built with one command.
- **UC-33:** As a *release manager*, I want docs (`docs/architecture/decisions/0004-multi-arch-linux-and-windows-arm64.md` and `docs/operations/arm64.md`) describing the supported platforms, the buildx + bake strategy, the CI runner matrix, the Compose `DOCKER_PLATFORM` opt-in pattern, the verification steps, and the Apple Silicon / Graviton / Windows on ARM quick-start, so a new operator can deploy the stack on any of the supported platforms without reverse-engineering the workflow file.

### Out of Scope (for this delta)

- **`linux/arm/v7` (32-bit ARM).** Deliberately excluded. Raspberry Pi OS in 32-bit mode and the original Raspberry Pi (Pi 1, Pi Zero) are out of scope for v1. A 64-bit Raspberry Pi 5 is in scope via `linux/arm64`.
- **Multi-OS Linux distributions** (Alpine vs. Debian vs. RHEL) as separate supported targets. The choice of base image flavor is an implementation detail of task 1.19; only the `linux/amd64` vs. `linux/arm64` vs. `windows/arm64` split is a user-visible platform contract.
- **Native Windows containers on Windows on ARM hosts** for the `api` (ASP.NET Core) or `web` (nginx + Angular bundle) images in the dev `docker compose` path. The dev path remains Linux containers (per the base spec); `windows/arm64` exists to support production deployments on Windows on ARM hosts that run Windows containers.
- **QEMU-based emulation in the recommended CI path.** The base spec's C.1 job matrix `linux-x64, win-x64` is superseded for the multi-arch delta by native-runner jobs.
- **EmguCV / camera integration redesign.** The base spec lists EmguCV removal as out of scope (a future spec); this delta inherits that boundary.
- **Cross-tenant analytics, billing, and mobile-native apps.** Inherited from the base spec's "Out of Scope" list.
