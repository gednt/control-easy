# Implementation Plan: CI API Readiness Reliability

**Branch**: `feat/fix-ci-openapi-wait` | **Date**: 2026-07-13 | **Spec**: [requirements.md](requirements.md)

## Summary

Make CI readiness work across host and Docker-container job runners, then pass runner-reachable URLs safely across steps.

## Technical Context

**Language/Version**: GitHub Actions YAML with Bash  
**Dependencies**: Docker Compose v2, curl, `$GITHUB_ENV`  
**Storage**: N/A  
**Testing**: Shell syntax and workflow structural assertions; Compose smoke  
**Platform**: GitHub-hosted Ubuntu  
**Constraints**: Gitea/Act containerized jobs; GitHub host jobs; dynamic ports; Angular working directories; 180-second bound
**Scope**: Two readiness blocks and two URL consumers

## Constitution Check

- **I Spec-Driven Development**: PASS — spec and tasks precede workflow edits.
- **IV Test-First & Verification**: PASS — structural checks and Compose smoke cover the workflow-only change.
- **V Runtime Container Parity**: PASS — Compose remains the runtime target.
- **VI Workflow Ownership**: PASS — artifacts stay under `.specs/`.
- **VII Concurrency Budget**: PASS — no sub-agents are used.

Post-design re-check: PASS. No exception is required.

## Project Structure

```text
.github/workflows/ci.yml
docker/docker-compose.yml
docker/reverse-proxy/dynamic.yml
.specs/fix-ci-api-readiness/{requirements,bugfix,plan,research,quickstart,tasks}.md
```

**Structure Decision**: Keep implementation in the existing workflow. Containerized jobs join the existing Compose network; host jobs retain published-port access.
