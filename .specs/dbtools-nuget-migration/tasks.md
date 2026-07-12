# Tasks — DBTools NuGet Migration

> Companion to `requirements.md`. Pin: [DBTools 1.4.3](https://www.nuget.org/packages/DBTools)

## Task Dependency Graph

```json
{
  "waves": [
    { "wave": 1, "tasks": ["1", "2"] },
    { "wave": 2, "tasks": ["3", "4"] },
    { "wave": 3, "tasks": ["5", "6"] },
    { "wave": 4, "tasks": ["7"] }
  ]
}
```

- [x] **1. Pin NuGet package in CPM**
  - Add `<PackageVersion Include="DBTools" Version="1.4.3" />` to `src/Directory.Packages.props`
  - Confirm `MySqlConnector` is already pinned (required MySQL provider per NuGet docs)

- [x] **2. Swap Infrastructure project reference**
  - In `src/BuildingBlocks/ControlEasyReborn.Infrastructure/ControlEasyReborn.Infrastructure.csproj`, replace `<ProjectReference Include="..\..\lib\DBTools_SQL\DBTools\DBTools.csproj" />` with `<PackageReference Include="DBTools" />`
  - Run `dotnet restore` and fix any namespace/API drift (expected: `DBTools.*` namespaces unchanged)

- [x] **3. Remove vendored source and solution entries**
- [x] **4. Update Docker build**
- [x] **5. Verify build and tests**
- [x] **6. Verify Docker stack**
- [x] **7. Update documentation**
  - Amend `docs/architecture/decisions/0002-dbtools-sql-as-only-data-access.md`
  - Update `AGENTS.md` data-access section (NuGet, not vendored path)
  - Refresh `.planning/codebase/STACK.md` and `CONCERNS.md` vendored-DBTools note

**Verification gate:**
- No `src/lib/DBTools_SQL/` in repo
- `git grep -i "lib/DBTools" src/ docker/` returns nothing (except historical docs if any)
- All tests green; API container healthy
