# Implementation Plan: Dashboard Visits and Access Registration

**Branch**: `feat/dashboard-visits-access-registration` | **Date**: 2026-09-13 | **Spec**: [spec.md](spec.md)

## Summary

Unify dashboard and Visits-page visit creation behind one standard modal that requires a destination apartment. Extend the tenant-scoped append-only access log with resident and vehicle exits, and expose the new event in the gatehouse workflow and audit UI.

## Technical Context

**Language/Version**: C# 12 / .NET 8 and TypeScript / Angular 18

**Primary Dependencies**: FluentValidation, DBTools, Angular Reactive Forms and signals

**Storage**: MySQL 8, existing Visits and ConsentAuditLog tables

**Testing**: xUnit with FluentAssertions and NSubstitute; Angular Karma/Jasmine

**Target Platform**: Docker Compose web platform

**Project Type**: Modular-monolith API and SPA

**Constraints**: Tenant-scoped DBTools data access; append-only access events; standard modal components; no EF Core

## Constitution Check

- Spec-driven artifacts are present for the feature.
- Existing tenant-aware repository is reused for access logging; no cross-tenant path is added.
- Data access remains DBTools based.
- Backend and Angular tests cover new exit behavior; final verification must run in Docker Compose.
- Access logs remain append-only and errors continue through the existing validation/ProblemDetails flow.

## Project Structure

```text
.specs/dashboard-visits-access/
src/Web/ControlEasyReborn.Web/src/app/features/visits/
src/Web/ControlEasyReborn.Web/src/app/features/dashboard/
src/Web/ControlEasyReborn.Web/src/app/features/entry-log/
src/Web/ControlEasyReborn.Web/src/app/design-system/components/
src/Modules/Photos/ControlEasyReborn.Modules.Photos.{Domain,Application,Infrastructure}/
tests/ControlEasyReborn.UnitTests/Modules/Photos/
```

**Structure Decision**: Reuse the Visits feature for the shared modal and the Photos access-audit module for movement events, avoiding a new module or table.
