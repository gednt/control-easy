# Demo Host Boot Root Cause (Task 6.4)

## Symptom

`DemoModeTests` fail with:

- `System.InvalidOperationException: The entry point exited without ever building an IHost.`

## Stack-trace anchor

The failure surfaces through `WebApplicationFactory` while trying to create the API host (`HostFactoryResolver`, `DeferredHostBuilder`).

## Root cause

`src/Host/ControlEasyReborn.Api/Program.cs` initializes Serilog bootstrap logging before host build and hard-loads `appsettings.json` using:

- `SetBasePath(Directory.GetCurrentDirectory())`
- `AddJsonFile("appsettings.json", optional: false, ...)`

When tests execute from a different working directory, `appsettings.json` is not guaranteed there. The bootstrap logger initialization throws before `WebApplication.CreateBuilder(args)` completes, so the entry point exits before creating `IHost`.

## Remediation

- Make bootstrap `appsettings.json` optional to prevent pre-host hard failure.
- Keep environment-specific file optional as already configured.

## Expected post-fix behavior

`WebApplicationFactory` can construct host, and demo integration tests proceed to functional assertions instead of startup crash.
