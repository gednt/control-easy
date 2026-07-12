# Requirements — Dev Containers & Worktrees

> Spec folder: `.specs/devcontainers/`
> Goal: Provide a **single, uniform local development shell** (devcontainer)
> and a **per-feature-branch worktree convention** so that every contributor
> — on every OS — boots the same way, runs the same verification gate, and
> can hold multiple in-flight branches in parallel without colliding on
> ports, hostnames, or databases.
>
> This spec is **additive** to the existing modernized stack. It does not
> change the runtime target (Docker Compose is still the runtime, per
> Constitution v1.1.0 Principle V). It changes only the **dev shell**.

## User Stories

### Reproducible shell

- **UC-D1:** As a *new contributor on Windows / macOS / Linux*, I want a
  single command (or a single "Reopen in Container" click in my IDE) that
  brings up a development shell with the right .NET 8 SDK, Node 20, the
  Angular CLI, Playwright browsers, the MySQL client, and the repo's
  pre-commit hooks, so I never have to read a 200-line `development-guide.md`
  to figure out which Node version is pinned.
- **UC-D2:** As a *devcontainer user*, I want the dev shell to invoke the
  **same** Docker Compose stack the host would invoke (Docker-out-of-Docker),
  so the verification gate ("rebuild and restart the stack, then test")
  produces the same artifacts in devcontainer mode as on the host. I do
  NOT want a separate, nested Docker engine that drifts from the host.

### Parallel branches

- **UC-D3:** As a *contributor with two feature branches in flight*, I
  want each branch to live in its own git worktree with its **own**
  Compose project (own images, own DB volume, own Traefik hostname, own
  published port), so a `dotnet ef migrations add` in branch A cannot
  break branch B's database, and a hot-reload in branch A cannot block
  a `docker compose logs` in branch B.
- **UC-D4:** As a *code reviewer*, I want to be able to spin up a
  reviewer's worktree from a pull request's branch URL in one command
  (or in a GitHub Codespace, which reuses the same devcontainer
  definition), so I can verify the PR's verification gate end-to-end
  without re-cloning.

### Photographic / hardware integration (fallback)

- **UC-D5:** As a *developer working on
  `.specs/3 - photo-capture-hardware-integration/`*, I want a clear
  documented fallback to the **host shell** for the specific subset of
  tasks that need USB / camera passthrough (which devcontainers do not
  support portably), so the devcontainer does not block that one spec.

---

## Out of Scope (for this spec)

- Replacing the Docker Compose runtime. (Constitution V forbids it; the
  devcontainer is a shell, not a runtime.)
- Adopting OpenSpec change-tracking. (`openspec/` is a stub today; the
  trigger for adoption is recorded as
  `TODO(CONSTITUTION_VI_OPENSPEC_ADOPTION_DATE)` in the constitution
  Sync Impact Report.)
- Continuous integration on a hosted runner. (`/gsd-modernization-roadmap`
  continuous task C.1 — GitHub Actions — is still "Active" in
  `.planning/PROJECT.md` and is out of scope for this spec; the
  devcontainer is local-only.)
- Replacing `.planning/`, `.specs/`, `.specify/`, or `openspec/` with a
  single tool. (Constitution v1.1.0 Principle VI fixes the split.)
- Retiring the legacy `docs/` generation pipeline. (The retirement
  schedule is in the constitution's "Documentation Systems and Source
  of Truth" section; it is a separate milestone-boundary action.)
- Renaming `docker-compose.yml` / `docker-compose.demo.yml` to
  `compose.yaml` / `compose.demo.yaml`. (Cosmetic; the Compose v2
  CLI accepts both, the existing names are stable, and the rename
  would invalidate dozens of references in this spec and elsewhere.)
