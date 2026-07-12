# Design — Dev Containers & Worktrees

## Overview

This spec adds two coupled primitives to the local development loop:

- a single [devcontainer](https://containers.dev/) definition
  (`.devcontainer/devcontainer.json` + a single Dockerfile
  `.devcontainer/Dockerfile`) that is the **canonical dev shell** for
  every contributor on every OS; and
- a per-feature-branch **git worktree** convention (enforced by the
  two shell scripts `scripts/worktree-up.sh` and
  `scripts/worktree-down.sh`) that gives every worktree its own
  Compose project name, Traefik hostname, published port, and DB
  volume.

Both primitives preserve the existing **runtime target** — the
`docker-compose.yml` stack is unchanged, and the verification gate
remains the canonical
`docker compose -p <project> build api web && docker compose -p <project> up -d --force-recreate api web && dotnet test src/ControlEasyReborn.sln`
sequence.

## Glossary

| Term | Meaning |
|---|---|
| **Dev shell** | The local development environment a contributor's terminal / IDE attaches to. Today this is the host OS. After this spec lands, it is a devcontainer. |
| **Runtime target** | The Docker Compose stack defined under `docker/`. Unchanged by this spec. |
| **Verification gate** | The command sequence that proves a task is done: build, up, test. Per Constitution IV and `AGENTS.md` "Post-task verification". |
| **DooD** | Docker-out-of-Docker. The devcontainer mounts the host's `/var/run/docker.sock` and runs `docker` against the host's engine. The devcontainer does NOT run its own Docker daemon. |
| **DinD** | Docker-in-Docker. The devcontainer runs a nested `dockerd` (typically via `docker:dind`). Adds an extra engine that can drift from the host. |
| **Worktree** | A `git worktree` checkout of a single branch, placed in a sibling directory of the main checkout. Multiple worktrees share the same `.git` database. |
| **Compose project** | The Compose v2 unit of isolation: a project name (`COMPOSE_PROJECT_NAME`), a set of containers, a set of networks, and a set of volumes. Per-worktree Compose projects are the isolation boundary for this spec. |
| **Traefik hostname** | A loopback-resolvable hostname (e.g. `ce-feat-x.localhost`) routed by Traefik's file provider. Per-worktree hostnames avoid Traefik router collisions. |
| **`scripts/worktree-up.sh`** | The shell script that creates a worktree, generates an env file, registers the Traefik hostname in `/etc/hosts` (or an alternative resolver), and runs the first `docker compose up -d --build`. |
| **`scripts/worktree-down.sh`** | The teardown script: `docker compose -p <project> down -v` for the worktree's project, then `git worktree remove`. Never touches the main checkout. |

## Decision: DooD vs DinD (and the third option)

The decision is recorded in this spec's `tasks.md` wave 1, but it is
worth surfacing in design because it constrains every other choice.

| Option | Description | Pros | Cons | Verdict |
|---|---|---|---|---|
| **A. DooD — mount `/var/run/docker.sock`** | Devcontainer mounts the host's Docker socket. The dev shell's `docker` and `docker compose` invocations target the host's engine. | Simplest. Same images, same volumes, same networks, same Traefik labels. Verification gate is literally identical. No nested engine. | The devcontainer can do anything the host's Docker user can do (intentional — the verification gate is the same engine). | **CHOSEN.** |
| **B. DinD — nested `dockerd` (privileged)** | Devcontainer runs `docker:dind` (or `docker:dind-rootless`). | Full isolation; the dev container cannot see host Docker state. | Slower start. Requires `--privileged` (or rootless + cgroups v2). Nested engine **will** drift: the host's `mysql:8.0` image is not visible to the nested engine. The verification gate becomes "works in the nested engine", which is NOT the runtime target. | **Rejected.** Violates Constitution V "Compose is the runtime target; dev shell is uniform, not parallel". |
| **C. No Docker in the devcontainer** | Devcontainer contains only the SDK + Node + Playwright. The verification gate shells out to the host's `docker compose` (via `host.docker.internal` or a `ssh` wrapper). | Most isolated dev shell. Smallest image. | Every verification-gate command has to know it is being run from inside a devcontainer and shell out. Slow feedback. Most surprising behavior for the contributor. | **Rejected** as a default; accepted as an opt-in `scripts/worktree-verify-from-host.sh` for CI parity. |

**Rationale (A is chosen):** Constitution v1.1.0 Principle V
demands that the verification gate produce the same result on the
dev shell as on the host runtime. DooD gives that for free: the
devcontainer's `docker compose` IS the host's `docker compose`.
DinD creates a second engine that can drift. Option C is
operationally correct but loses the "one command" experience.

**Worktree integration:** DooD composes naturally with the worktree
convention. Each worktree sets `COMPOSE_PROJECT_NAME` to a
per-worktree value; the host's engine produces per-worktree
containers, networks, and volumes; the devcontainer shell sees all of
them (because it shares the host's socket). No additional plumbing.

## Base image stack

The devcontainer image is built from a single Dockerfile at
`.devcontainer/Dockerfile`. It is built on top of
`mcr.microsoft.com/devcontainers/base:debian-12` (the official
devcontainers base for Debian 12 — stable, small, supports Docker
CLI out of the box) and adds the layers below.

| Layer | Source | Purpose | Why this version |
|---|---|---|---|
| Base | `mcr.microsoft.com/devcontainers/base:debian-12` | Common devcontainer utilities (git, curl, sudo, zsh, common editors) | Debian 12 (bookworm) is the current stable; matches the .NET 8 runtime base. |
| .NET SDK | `mcr.microsoft.com/dotnet/sdk:8.0` | Restore, build, test | Pinned in `src/global.json` to 8.0.0 with `rollForward: latestMajor`. |
| Node.js | `node:20-bookworm` | Angular CLI, npm scripts, Playwright | Pinned in `src/Web/ControlEasyReborn.Web/package.json` engines.node. |
| MySQL client | Debian `default-mysql-client` | `mysql` shell against the dev / test DB | Matches `mysql:8.0` server image used in Compose. |
| Playwright | `mcr.microsoft.com/playwright:v1.49.x-jammy` (or installed via `npx playwright install`) | E2E browser tests | Matches `package.json` Playwright pin. |
| Docker CLI + Compose plugin | `docker-ce-cli` + `docker-compose-plugin` (from Docker's official apt repo) | `docker compose` invocations against the host's socket | Required for DooD. |
| shellcheck, hadolint | Debian packages | Pre-commit / CI lint of `scripts/*.sh` and `Dockerfile` | Cheap to add; catches the most common script bugs. |
| repo pre-commit hook | `scripts/install-devcontainer-hooks.sh` | Format / lint on commit | Lives in the repo, not the image, so it tracks the repo. |

The devcontainer image is **not** built every `postCreateCommand`; it
is built once when `.devcontainer/Dockerfile` changes (the devcontainer
CLI detects the change and rebuilds). The build is layer-cached on
the local Docker engine so subsequent rebuilds are fast.

## Worktree convention

### Per-worktree Compose project

Each worktree exports a `COMPOSE_PROJECT_NAME` of
`ce-<branch-with-slashes-as-hyphens>`. This isolates the worktree's
containers, networks, and volumes from the main checkout and from
every other worktree. The shell scripts in `scripts/` derive the
project name from the current branch via
`git rev-parse --abbrev-ref HEAD | tr / -`.

### Per-worktree Traefik hostname

Each worktree adds an entry to `/etc/hosts` (or to a local DNS
resolver like `dnsmasq`) for `ce-<branch>.localhost` → `127.0.0.1`,
and the worktree's `docker-compose.override.yml` patches the
Traefik router labels to use the per-worktree hostname. The
override file is auto-generated by `scripts/worktree-up.sh` and
removed by `scripts/worktree-down.sh`. **It is gitignored.**

### Per-worktree port range

The base port for the Traefik HTTP listener is `18080` (deliberately
above the historical `:8080` used by the main checkout). Worktree
N uses `18080 + N * 10`. This is the only place port allocation
lives; the verification-gate commands reference it via
`scripts/worktree-port.sh`.

### Per-worktree DB volume

The Compose v2 `docker-compose.yml` declares a named volume
(`mysql-data`). The override sets the volume name to
`ce-<branch>-mysql-data`. This is what gives the worktree its
own database state; the volume is removed by
`docker compose -p <project> down -v`.

### Worktree bring-up sequence

```
git worktree add -b feat/<id> ../ControlEasy.<id> main
cd ../ControlEasy.<id>
COMPOSE_PROJECT_NAME=ce-<id> ./scripts/worktree-up.sh
```

`scripts/worktree-up.sh` performs:

1. Detect the worktree's branch (sanity check).
2. Compute the worktree index (sanity check vs `git worktree list`).
3. Generate `docker/.env.worktree.<id>` with the right
   `COMPOSE_PROJECT_NAME`, `TRAEFIK_HOST`, `TRAEFIK_PORT`,
   `MYSQL_VOLUME_NAME`, `JWT_SIGNING_KEY` (per-worktree; do NOT
   reuse the main checkout's key).
4. Append the Traefik hostname to `/etc/hosts` (idempotent; backs
   up and restores on teardown).
5. Generate `docker/docker-compose.worktree.<id>.yml` (the
   per-worktree override; gitignored).
6. Run the verification gate:
   `docker compose -p ce-<id> -f docker/docker-compose.yml -f docker/docker-compose.worktree.<id>.yml up -d --build`.
7. Print the URLs (`https://ce-<id>.localhost:18080+10N/`,
   `/api/v1/health`, `/db`, `/swagger`).

`scripts/worktree-down.sh` performs:

1. `docker compose -p ce-<id> down -v` (removes ONLY the worktree's
   containers, networks, and volume).
2. Remove the `/etc/hosts` entry (restores the backup).
3. `git worktree remove --force ../ControlEasy.<id>`.

## Architecture diagram

```text
                 ┌──────────────────────────────────────────────────┐
                 │              Host (Windows/macOS/Linux)            │
                 │  ┌────────────┐       ┌───────────────────────┐    │
                 │  │   IDE      │       │  Docker engine (host) │    │
                 │  │ (VS Code,  │       │  ┌────────────────┐   │    │
                 │  │  Cursor,   │       │  │ main checkout  │   │    │
                 │  │  JetBrains)│       │  │ project=ce     │   │    │
                 │  └─────┬──────┘       │  │ :8080          │   │    │
                 │        │ attach       │  └────────────────┘   │    │
                 │        ▼              │  ┌────────────────┐   │    │
                 │  ┌────────────┐  sock │  │ worktree A     │   │    │
                 │  │ devcontainer│ ◀────▶│  │ project=ce-a   │   │    │
                 │  │ (debian 12)│       │  │ :18090         │   │    │
                 │  │ .NET 8 SDK │       │  └────────────────┘   │    │
                 │  │ Node 20    │       │  ┌────────────────┐   │    │
                 │  │ Playwright │       │  │ worktree B     │   │    │
                 │  │ docker CLI │       │  │ project=ce-b   │   │    │
                 │  └────────────┘       │  │ :18100         │   │    │
                 │                       │  └────────────────┘   │    │
                 │                       └───────────────────────┘    │
                 └──────────────────────────────────────────────────┘
```

The devcontainer is the shell; the host's Docker engine is the engine;
the per-worktree Compose project is the unit of isolation.

## Interaction with the existing CI gap (C.1)

`.planning/PROJECT.md` "Active" lists continuous task **C.1** (GitHub
Actions) as not implemented. This spec does **not** close C.1. It
only ensures that the local dev loop is reproducible.

If/when C.1 lands, the same `.devcontainer/Dockerfile` is the
candidate for the CI job image. The verification gate then runs
in CI against the same image, and the "works on my machine but
not in CI" class of bug is closed by the same primitive. This is
explicitly called out in the Constitution Sync Impact Report
(`TODO(CONSTITUTION_VI_DOCS_RETIREMENT_DATE)` follow-up).

## Risks & Mitigations

| Risk | Mitigation |
|---|---|
| DooD means the devcontainer can do anything the host's Docker user can do (incl. removing the main checkout's stack). | `scripts/worktree-up.sh` sets `COMPOSE_PROJECT_NAME` and only ever invokes `docker compose -p ce-<branch> ...`; the project name is the guard. Code review enforces. |
| `/etc/hosts` edits on Windows require admin and don't survive reboots cleanly. | `scripts/worktree-up.sh` writes to a per-user `%USERPROFILE%/.control-easy/hosts` shim and uses the OS-native resolver (PowerShell `Add-DnsClientNrptRule` on Windows, `dscacheutil` on macOS, `nss-systemd` or `dnsmasq` on Linux) — fallback to `/etc/hosts` documented. |
| Per-worktree port collisions on the same host. | The port allocator is a function in `scripts/worktree-port.sh`; collisions abort the bring-up. |
| Devcontainer image becomes stale relative to the host's `docker-compose.yml` (e.g. a new base image in `Directory.Packages.props` is not in the image). | `scripts/worktree-up.sh` runs `docker compose build` after `docker compose pull`, so the stack is always up to date even if the devcontainer image is not. |
| Multiple devcontainer-aware IDEs (VS Code, Cursor, JetBrains) have slightly different `devcontainer.json` semantics. | The `devcontainer.json` is written to the spec-kit-compatible subset (the intersection of VS Code, Cursor, and JetBrains' devcontainer CLI). IDE-specific extensions go in `.vscode/extensions.json` and `.idea/`, not in the devcontainer. |
| `.specs/3 - photo-capture-hardware-integration/` needs USB / camera passthrough that devcontainers do not support. | UC-D5 documents the fallback (host shell + host browser) for that one spec. The devcontainer's `runArgs` do NOT request `--privileged` or `--device`; the photo-capture spec is the only place that needs them. |
