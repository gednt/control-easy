<!-- generated-by: gsd-doc-writer -->
# Testing Guide — ControlEasy Reborn

This guide covers the testing strategy, frameworks, execution instructions, and test creation standards for ControlEasy Reborn.

## Testing Strategy & Frameworks

The testing suite is designed to ensure strict adherence to Clean Architecture boundaries, multi-tenant data isolation, business logic correctness, and frontend UI accessibility.

### Frameworks Overview

| Layer | Framework & Tools | Project / Directory |
|---|---|---|
| **Unit Testing** | xUnit 2.8+, FluentAssertions, NSubstitute | `tests/ControlEasyReborn.UnitTests/` |
| **Architecture Rules** | NetArchTest.Rules | `tests/ControlEasyReborn.ArchitectureTests/` |
| **Integration Testing** | xUnit + Testcontainers.MySql | `tests/ControlEasyReborn.IntegrationTests/` |
| **Frontend Unit** | Karma + Jasmine + ChromeHeadless | `src/Web/ControlEasyReborn.Web/` (`src/**/*.spec.ts`) |
| **E2E & Accessibility** | Playwright + `@axe-core/playwright` | `src/Web/ControlEasyReborn.Web/` (`tests/`) |

---

## Running Tests

### 1. Backend Tests (.NET)

Run tests inside the container environment or devcontainer:

```bash
# Run unit tests across all 10 modules
dotnet test tests/ControlEasyReborn.UnitTests

# Run Clean Architecture boundary & rule validation
dotnet test tests/ControlEasyReborn.ArchitectureTests

# Run database integration tests using Docker Testcontainers
dotnet test tests/ControlEasyReborn.IntegrationTests
```

To run all backend tests sequentially:
```bash
make test
```

### 2. Frontend Unit Tests (Angular)

From `src/Web/ControlEasyReborn.Web`:
```bash
# Run Karma tests once in headless mode
npm test -- --no-watch --browsers=ChromeHeadless

# Run Karma in continuous watch mode during development
npm test
```

### 3. End-to-End Tests (Playwright)

From `src/Web/ControlEasyReborn.Web`:
```bash
# Install required browser binaries
npm run e2e:install

# Run Playwright E2E suite
npm run e2e
```

---

## Writing New Tests

### Unit Tests
- Follow the **Arrange-Act-Assert (AAA)** pattern.
- Test names should follow `MethodName_Scenario_ExpectedBehavior` (e.g., `RegisterResident_WithDuplicateDocument_ThrowsValidationException`).
- Mock all out-of-process and infrastructural dependencies using `NSubstitute` (`Substitute.For<ITenantAwareLinqFactory>()`).
- Never perform database I/O in unit test assemblies.

### Architecture Tests
- Located in `tests/ControlEasyReborn.ArchitectureTests/`.
- Verify architectural invariants using `NetArchTest.Rules`:
  - Domain layer must not depend on Application, Infrastructure, or Api.
  - Application layer must not depend on Infrastructure or Api.
  - Feature modules must not reference internal implementations of sibling modules.
  - All repository and service implementations must be marked `sealed`.

Example rule definition:
```csharp
[Fact]
public void Domain_ShouldNot_DependOn_OtherLayers()
{
    var result = Types.InAssembly(typeof(Resident).Assembly)
        .ShouldNot()
        .HaveDependencyOn("ControlEasyReborn.Modules.Residents.Infrastructure")
        .GetResult();

    result.IsSuccessful.Should().BeTrue();
}
```

### Integration Tests
- Located in `tests/ControlEasyReborn.IntegrationTests/`.
- Use `Testcontainers.MySql` to spin up a genuine MySQL 8 container during test execution.
- Tests execute real DBTools LINQ queries and migrations against a live database instance to catch SQL syntax, constraint, or provider issues before deployment.

### Angular Component Tests
- Test standalone components using `TestBed.configureTestingModule()`.
- Use signals for mock inputs and outputs.
- Verify template interactions and accessibility attributes.

---

## Continuous Integration (CI)

The GitHub Actions workflow (`.github/workflows/ci.yml`) validates every Pull Request across five parallel jobs:

| Job | Trigger | Tasks Executed |
|---|---|---|
| `lint` | PR / Push to main | `dotnet format --verify-no-changes`, `npm run lint`, `npm test` (ChromeHeadless) |
| `build` | PR / Push to main | `dotnet build`, tokens contract tests, `ng build --configuration production` |
| `test-fast` | PR / Push to main | Unit tests + NetArchTest architecture validation |
| `test-integration` | PR / Push to main | Integration tests with cached MySQL 8 Testcontainers image |
| `openapi` | PR / Push to main | In-process API boot, OpenAPI schema generation, `ng-openapi-gen` drift check |
| `docker` | Push to main (after tests) | Multi-stage Docker image builds and GHCR publishing |

All checks must be green before merging into `main`.
