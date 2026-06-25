# Testing Patterns

**Analysis Date:** 2026-06-24

## Overview

Testing spans four layers:

| Layer | Location | Framework |
|-------|----------|-----------|
| .NET unit tests | `tests/ControlEasyReborn.UnitTests/` | xUnit + FluentAssertions + NSubstitute |
| .NET integration tests | `tests/ControlEasyReborn.IntegrationTests/` | xUnit + Testcontainers + WebApplicationFactory |
| .NET architecture tests | `tests/ControlEasyReborn.ArchitectureTests/` | xUnit + NetArchTest.Rules |
| Angular unit tests | `src/Web/ControlEasyReborn.Web/src/**/*.spec.ts` | Jasmine + Karma |
| E2E / visual / a11y | `src/Web/ControlEasyReborn.Web/e2e/`, `tests/visual/`, `tests/a11y/` | Playwright |

Legacy ControlEasy 5 / ControlEasyWeb have no test projects in this repository.

Solution entry point: `src/ControlEasyReborn.sln` (includes test projects under `tests/`).

---

## .NET Test Framework

**Runner:**
- xUnit 2.9.x (`xunit`, `xunit.runner.visualstudio`, `Microsoft.NET.Test.Sdk`)
- Central versions in `src/Directory.Packages.props`; tests import via `tests/Directory.Packages.props`

**Assertion library:**
- FluentAssertions 8.x — prefer fluent `.Should()` chains over raw `Assert.*`

**Mocking:**
- NSubstitute 5.x for interface mocks (`Substitute.For<T>()`, `.Returns()`, `.Received()`)
- Custom test doubles in `tests/ControlEasyReborn.UnitTests/TestDoubles/` for DBTools (`FakeAsyncSqlClient`, `FakeTenantContext`, `FakeTenantAwareLinqFactory`)

**Architecture testing:**
- NetArchTest.Rules 1.3.x for layer dependency and naming enforcement

**Run commands:**
```bash
# All .NET tests (from repo root)
dotnet test src/ControlEasyReborn.sln

# Single project
dotnet test tests/ControlEasyReborn.UnitTests/ControlEasyReborn.UnitTests.csproj
dotnet test tests/ControlEasyReborn.IntegrationTests/ControlEasyReborn.IntegrationTests.csproj
dotnet test tests/ControlEasyReborn.ArchitectureTests/ControlEasyReborn.ArchitectureTests.csproj

# Filter by test name
dotnet test tests/ControlEasyReborn.UnitTests --filter "FullyQualifiedName~CreateApartmentHandler"
```

**Build settings** (`tests/Directory.Build.props`):
- Same `net8.0`, nullable, `TreatWarningsAsErrors` as production code
- `IsTestProject=true`, `IsPackable=false`

---

## .NET Test File Organization

**Location:**
- Separate `tests/` tree mirroring module structure — not co-located with `src/`
- Unit tests: `tests/ControlEasyReborn.UnitTests/Modules/{Module}/`, `MultiTenancy/`, `Hosting/`, `Demo/`
- Integration tests: flat or grouped by feature at project root (e.g. `ResidentEndpointTests.cs`, `VisitEndpointTests.cs`)
- Architecture tests: `tests/ControlEasyReborn.ArchitectureTests/`

**Naming:**
- Test class: `{TypeUnderTest}Tests` (e.g. `CreateApartmentHandlerTests`, `ApartmentRepositoryTests`)
- Test method (unit): `{MethodName}_with_{condition}_{expectedOutcome}` — examples:
  - `HandleAsync_with_valid_request_creates_apartment`
  - `HandleAsync_with_duplicate_block_unit_throws_ConflictException`
  - `FindAsync_sends_select_with_id_parameter`
- Integration tests: `{Action}_{Context}_Returns{Status}` or `CrossTenant_{Action}_{Expected}`:
  - `CreateResident_AsTenantA_ReturnsCreated`
  - `CrossTenant_GetResident_AsDifferentTenant_Returns404`

**Structure:**
```
tests/
├── ControlEasyReborn.UnitTests/
│   ├── Modules/
│   │   ├── Apartments/
│   │   │   ├── CreateApartmentHandlerTests.cs
│   │   │   ├── ApartmentRepositoryTests.cs
│   │   │   └── CreateApartmentRequestValidatorTests.cs
│   │   ├── Residents/
│   │   ├── Security/
│   │   └── Visits/
│   ├── MultiTenancy/
│   ├── TestDoubles/
│   │   └── FakeAsyncSqlClient.cs
│   └── ControlEasyReborn.UnitTests.csproj
├── ControlEasyReborn.IntegrationTests/
│   ├── MySqlContainerFixture.cs
│   ├── TestcontainersWebApplicationFactory.cs
│   ├── TenantAwareWebApplicationFactory.cs
│   ├── DemoWebApplicationFactory.cs
│   ├── ResidentEndpointTests.cs
│   └── ControlEasyReborn.IntegrationTests.csproj
└── ControlEasyReborn.ArchitectureTests/
    ├── LayerDependencyTests.cs
    ├── CrossTenantTestNamingTests.cs
    └── ControlEasyReborn.ArchitectureTests.csproj
```

---

## .NET Unit Test Structure

**Suite organization:**
```csharp
using FluentAssertions;
using NSubstitute;
using Xunit;

namespace ControlEasyReborn.UnitTests.Modules.Apartments;

public sealed class CreateApartmentHandlerTests
{
    private readonly IApartmentRepository _apartments;
    private readonly IValidator<CreateApartmentRequest> _validator;
    private readonly CreateApartmentHandler _sut;

    public CreateApartmentHandlerTests()
    {
        _apartments = Substitute.For<IApartmentRepository>();
        _validator = new CreateApartmentRequestValidator();
        _sut = new CreateApartmentHandler(_apartments, _validator);
    }

    [Fact]
    public async Task HandleAsync_with_valid_request_creates_apartment()
    {
        _apartments.FindByBlockUnitAsync("A", "101", Arg.Any<CancellationToken>())
            .Returns((Apartment?)null);

        var response = await _sut.HandleAsync(
            new CreateApartmentRequest("A", "101"), Guid.NewGuid(), CancellationToken.None);

        response.Block.Should().Be("A");
        await _apartments.Received(1).AddAsync(
            Arg.Is<Apartment>(a => a.Block == "A"), Arg.Any<CancellationToken>());
    }
}
```

**Patterns:**
- Use constructor for shared setup (xUnit creates new instance per test)
- Name SUT `_sut` (system under test)
- Use real validators when testing handlers; mock only repositories/external ports
- `CancellationToken.None` in unit tests unless cancellation behavior is under test
- Async tests: return `Task`, use `await act.Should().ThrowAsync<TException>()`

**Repository unit tests:**
- Use `FakeAsyncSqlClient` to capture SQL operations without a database
- Assert on `_fakeClient.Operations` — SQL text and parameters (see `ApartmentRepositoryTests.cs`)
- Wire tenant filtering via `TenantFilterInterceptor` + `FakeTenantContext`

**Error testing:**
```csharp
var act = () => _sut.HandleAsync(request, tenantId, CancellationToken.None);

await act.Should().ThrowAsync<ConflictException>()
    .WithMessage("*already exists*");
```

---

## .NET Integration Test Structure

**Infrastructure:**
- `MySqlContainerFixture` (`tests/ControlEasyReborn.IntegrationTests/MySqlContainerFixture.cs`):
  - Testcontainers `mysql:8.0` image
  - Runs all `docker/mysql/init/*.sql` scripts in order
  - Shared via `[Collection("MySql Collection")]` + `ICollectionFixture<MySqlContainerFixture>`
- `TestcontainersWebApplicationFactory` extends `TenantAwareWebApplicationFactory`:
  - `WebApplicationFactory<Program>` bootstraps full API
  - Overrides `Db:*` and `Jwt:*` config from container connection string
- `TenantAwareWebApplicationFactory`:
  - `AsTenantA()`, `AsTenantB()`, `AsPlatformAdmin()` return `HttpClient` with JWT bearer tokens
  - Fixed tenant GUIDs: `TenantAId`, `TenantBId`, `PlatformAdminId`
- `JwtTestHelper` — generates test JWTs (referenced by factories)
- `DemoWebApplicationFactory` — demo mode seed tests

**Suite organization:**
```csharp
[Collection("MySql Collection")]
public sealed class ResidentEndpointTests
{
    private readonly TestcontainersWebApplicationFactory _factory;

    public ResidentEndpointTests(MySqlContainerFixture mySql)
    {
        _factory = new TestcontainersWebApplicationFactory(mySql);
    }

    [Fact]
    public async Task CreateResident_AsTenantA_ReturnsCreated()
    {
        var client = _factory.AsTenantA();
        var response = await client.PostAsJsonAsync("/api/v1/residents", request);
        response.StatusCode.Should().Be(HttpStatusCode.Created);
    }
}
```

**Patterns:**
- Use `HttpClient` from factory helpers; do not manually craft auth unless testing auth edge cases
- Assert HTTP status with `FluentAssertions` + `System.Net.HttpStatusCode`
- Deserialize with `ReadFromJsonAsync<T>()`
- Cross-tenant isolation tests **required** for any integration class that touches repositories — enforced by `CrossTenantTestNamingTests` (method name must match `CrossTenant_.*`)
- Demo-specific flows: `DemoWebApplicationFactory` + login helpers inside test class

**What to mock:**
- Nothing inside the API stack — real MySQL, real handlers, real middleware
- External services not part of the monolith are not present; full stack is exercised

---

## .NET Architecture Tests

**Purpose:** Enforce structural rules at build time.

**Examples** (`tests/ControlEasyReborn.ArchitectureTests/`):
- `LayerDependencyTests` — Domain must not reference Infrastructure; Infrastructure must not reference EF Core or `MySql.Data`
- `CrossTenantTestNamingTests` — integration classes touching repositories must include a `CrossTenant_*` `[Fact]`
- `SchemaBackfillSyncTests` — new business tables must appear in tenant backfill scripts
- `TenantIdPropertyTests` — entity tenant ID conventions

**Pattern:**
```csharp
var result = Types.InAssemblies(domainAssemblies)
    .ShouldNot()
    .HaveDependencyOnAny("ControlEasyReborn.Modules.Residents.Infrastructure")
    .GetResult();

result.IsSuccessful.Should().BeTrue();
```

---

## Angular Unit Tests

**Runner:**
- Karma 6.x + Jasmine 5.x via `@angular-devkit/build-angular:karma`
- Config in `angular.json` → `projects.controleasy-reborn-web.architect.test`
- TypeScript config: `tsconfig.spec.json`

**Run commands:**
```bash
cd src/Web/ControlEasyReborn.Web
npm test                    # ng test (Karma, watch mode by default)
npm test -- --no-watch --browsers=ChromeHeadless   # CI-style single run
```

**File organization:**
- Co-located `*.spec.ts` next to source (primarily `src/app/design-system/`)
- Feature modules under `src/app/features/` currently have **no** spec files — design-system is the reference
- Default schematics set `skipTests: true`; add specs manually for new design-system components

**Structure:**
```typescript
import { ComponentFixture, TestBed } from '@angular/core/testing';
import { CeButtonComponent } from './button.component';

describe('CeButtonComponent', () => {
  let fixture: ComponentFixture<CeButtonComponent>;
  let component: CeButtonComponent;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [CeButtonComponent],
    }).compileComponents();

    fixture = TestBed.createComponent(CeButtonComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('should have default variant=primary', () => {
    expect(component.variant()).toBe('primary');
  });
});
```

**Patterns:**
- Import standalone component under test in `TestBed.configureTestingModule({ imports: [...] })`
- Use `fixture.componentRef.setInput('prop', value)` for signal inputs
- Query DOM via `fixture.nativeElement.querySelector(...)`
- Parameterized cases: `['sm', 'md', 'lg'].forEach(size => { it(...) })`
- Services: `TestBed.inject(ThemeService)`; clear `localStorage` in `beforeEach`/`afterEach` when testing persistence

**Mocking:**
- No global mock library; stub dependencies via TestBed providers when needed
- HTTP services not yet unit-tested in features — integration covered by Playwright E2E

**Coverage:**
- `karma-coverage` package present; no enforced threshold in config
- Run with `ng test --code-coverage` for Istanbul reports

---

## Playwright E2E / Visual / A11y Tests

**Framework:** Playwright 1.49.x (`@playwright/test`, `@axe-core/playwright`)

**Config:** `src/Web/ControlEasyReborn.Web/playwright.config.ts`
- `testDir`: `./e2e`, `../../tests/visual`, `../../tests/a11y`
- `globalSetup`: `./e2e/global-setup.ts`
- `baseURL`: `process.env.E2E_BASE_URL ?? 'http://localhost:8080'`
- Snapshots: `tests/visual/__snapshots__/`
- CI: `retries: 2`, `forbidOnly: true`

**Run commands:**
```bash
cd src/Web/ControlEasyReborn.Web
npm run e2e:install          # playwright install chromium
npm run e2e                  # playwright test

# Against running docker stack
E2E_BASE_URL=http://localhost:8080 npm run e2e

# Single spec
npx playwright test e2e/first-boot-login.spec.ts
```

**E2E specs** (`src/Web/ControlEasyReborn.Web/e2e/`):
- `first-boot-login.spec.ts` — platform admin bootstrap + password change
- `apartments-crud.spec.ts`, `resident-create.spec.ts`, `vehicle-create.spec.ts`
- `visit-checkin-checkout.spec.ts`, `platform-condominiums.spec.ts`, `module-pages.spec.ts`
- `apartment-residents.spec.ts`

**E2E patterns:**
```typescript
import { test, expect } from '@playwright/test';

test.describe('First-boot PlatformAdmin login', () => {
  test('bootstrap credentials reach change-password then dashboard', async ({ page, request }) => {
    const bootstrap = await request.get(`${baseUrl}/api/v1/security/bootstrap`);
    test.skip(!(await bootstrap.json()).pending, 'Bootstrap credentials are not pending');

    await page.goto(`${baseUrl}/login`);
    await page.getByRole('button', { name: 'Sign in' }).click();
    await expect(page).toHaveURL(`${baseUrl}/change-password`);
  });
});
```

- Use `getByRole`, `getByLabel`, `getByText` for accessible selectors
- `test.skip(condition, reason)` for environment-dependent cases (bootstrap state)
- Env vars: `E2E_BASE_URL`, `BOOTSTRAP_EMAIL`, `BOOTSTRAP_PASSWORD`

**Visual regression** (`tests/visual/showcase.spec.ts`):
- Nested loops over viewports (mobile/tablet/desktop) and themes (light/dark)
- `await expect(page).toHaveScreenshot(...)` with `maxDiffPixelRatio: 0.001`
- Snapshots stored under `tests/visual/__snapshots__/`

**Accessibility** (`tests/a11y/showcase.spec.ts`):
- `AxeBuilder` with WCAG 2.x tags
- Fail on `serious` or `critical` violations only
- Logs violation details to console on failure

---

## Mockup Manual Testing

`mockup/SMOKE.md` defines a manual checklist for the static HTML prototype — not automated. Serve with `python -m http.server` from `mockup/`. Use for design validation only.

---

## Coverage Requirements

| Area | Enforcement |
|------|-------------|
| .NET line/branch coverage | Not enforced in CI (no coverlet config detected) |
| Angular Karma coverage | Optional via `--code-coverage`; no threshold |
| E2E | Required for major user flows; run against Docker stack per `AGENTS.md` post-task verification |
| Architecture tests | Required — `dotnet test` must pass before merge |
| Cross-tenant tests | Required by `CrossTenantTestNamingTests` for repository-touching integration classes |

**Post-implementation verification** (from `AGENTS.md`):
```bash
docker compose -f docker/docker-compose.yml build api web
docker compose -f docker/docker-compose.yml up -d --force-recreate api web
dotnet test src/ControlEasyReborn.sln
cd src/Web/ControlEasyReborn.Web && npm run e2e
```

---

## What to Mock — Summary

| Test type | Mock |
|-----------|------|
| Handler unit | Mock `I*Repository`; use real FluentValidation validators |
| Repository unit | `FakeAsyncSqlClient` + interceptors; no database |
| Integration | Nothing in API/DB path; real Testcontainers MySQL |
| Angular unit | TestBed providers for dependencies; no HttpClient mocking in existing specs |
| E2E | No mocking; full browser + API + DB |

**Do not mock:**
- FluentValidation rules in handler tests (use real validator classes)
- Domain entity behavior in entity tests (pure C# tests, no mocks)
- Internal handler logic — test through public `HandleAsync`

---

## Adding New Tests — Checklist

**New API module:**
1. Handler tests in `tests/ControlEasyReborn.UnitTests/Modules/{Module}/`
2. Validator tests for each request type
3. Repository tests with `FakeAsyncSqlClient` if custom SQL
4. Integration endpoint tests in `tests/ControlEasyReborn.IntegrationTests/{Module}EndpointTests.cs`
5. At least one `CrossTenant_*` test if class touches data layer
6. Register new tables in backfill script or `SchemaBackfillSyncTests` will fail

**New Angular feature:**
1. E2E spec in `e2e/{feature}.spec.ts` for CRUD/happy path
2. Design-system components: co-located `*.spec.ts` with Karma
3. Run `npm run e2e` against Docker stack on port 8080

**New architecture constraint:**
1. Add NetArchTest rule in `tests/ControlEasyReborn.ArchitectureTests/`

---

*Testing analysis: 2026-06-24*
*Update when test patterns change*
