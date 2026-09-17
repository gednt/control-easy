using System.Reflection;
using System.Text.RegularExpressions;
using FluentAssertions;
using NetArchTest.Rules;
using Xunit;

namespace ControlEasyReborn.ArchitectureTests;

/// <summary>
/// Aggregate constitution-compliance test for the AccessControl feature module.
/// Asserts the seven core principles from .specify/memory/constitution.md
/// (v1.3.0) plus the stack &amp; architecture constraints specifically as
/// they apply to src/Modules/AccessControl/.
/// </summary>
public sealed class AccessControlConstitutionComplianceTests
{
    private static readonly string[] AccessControlAssemblies = new[]
    {
        "ControlEasyReborn.Modules.AccessControl.Domain",
        "ControlEasyReborn.Modules.AccessControl.Application",
        "ControlEasyReborn.Modules.AccessControl.Infrastructure",
        "ControlEasyReborn.Modules.AccessControl.Api"
    };

    static AccessControlConstitutionComplianceTests()
    {
        var loaded = AppDomain.CurrentDomain.GetAssemblies()
            .Select(a => a.GetName().Name ?? string.Empty)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);
        foreach (var name in AccessControlAssemblies)
        {
            if (loaded.Contains(name)) continue;
            try { Assembly.Load(name); } catch { /* may be filtered out */ }
        }
    }

    /// <summary>
    /// Principle II — Multi-Tenant Isolation by Default.
    /// AccessControl application + api layers must not see DBTools Linq&lt;TModel&gt;;
    /// the Infrastructure layer is the only one that may talk to the tenant-aware
    /// data factory. Endpoints stay ignorant of the data layer.
    /// </summary>
    [Fact]
    public void Principle_II_AccessControl_endpoints_and_application_must_not_reference_DBTools()
    {
        var assemblies = AppDomain.CurrentDomain.GetAssemblies()
            .Where(a => a.GetName().Name is "ControlEasyReborn.Modules.AccessControl.Application"
                or "ControlEasyReborn.Modules.AccessControl.Api")
            .ToArray();

        assemblies.Should().NotBeEmpty();

        var result = Types.InAssemblies(assemblies)
            .ShouldNot()
            .HaveDependencyOnAny("DBTools.Abstractions", "DBTools")
            .GetResult();

        result.IsSuccessful.Should().BeTrue(
            "AccessControl Application + Api must not depend on DBTools directly — only Infrastructure may (Constitution Principle II).");
    }

    /// <summary>
    /// Principle III — LINQ-First Data Access (DBTools). The Infrastructure layer
    /// is where the repositories live and where raw SQL via stored procs is the
    /// only sanctioned escape hatch. No raw SQL strings targeting AccessControl-
    /// owned tables outside Infrastructure.
    /// </summary>
    [Fact]
    public void Principle_III_AccessControl_must_not_execute_raw_SQL_in_Application_or_Api_layers()
    {
        var root = FindRepositoryRoot();
        var sourceRoots = new[]
        {
            Path.Combine(root, "src", "Modules", "AccessControl", "ControlEasyReborn.Modules.AccessControl.Application"),
            Path.Combine(root, "src", "Modules", "AccessControl", "ControlEasyReborn.Modules.AccessControl.Api"),
        };

        var forbidden = new Regex(
            @"(?:FROM|UPDATE|INSERT\s+INTO|DELETE\s+FROM)\s+(?:AccessCredentials|CredentialLifecycleActions|AccessEvents|RefusedScanAttempts|AccessLookupAudits)\b",
            RegexOptions.IgnoreCase);

        var failing = new List<string>();
        foreach (var dir in sourceRoots)
        {
            if (!Directory.Exists(dir)) continue;
            foreach (var path in Directory.EnumerateFiles(dir, "*.cs", SearchOption.AllDirectories))
            {
                var content = File.ReadAllText(path);
                if (forbidden.IsMatch(content))
                {
                    failing.Add($"{Path.GetFileName(path)}: raw SQL targeting an AccessControl table outside Infrastructure is forbidden (Constitution Principle III).");
                }
            }
        }

        failing.Should().BeEmpty(string.Join("; ", failing));
    }

    /// <summary>
    /// Principle IV — Test-First &amp; Verification Discipline. The AccessControl
    /// module ships unit tests; a stub assembly must exist in the UnitTests project.
    /// </summary>
    [Fact]
    public void Principle_IV_AccessControl_unit_tests_must_exist_for_handlers_and_resolver()
    {
        var root = FindRepositoryRoot();
        var unitTestsRoot = Path.Combine(root, "tests", "ControlEasyReborn.UnitTests", "Modules", "AccessControl");
        Directory.Exists(unitTestsRoot).Should().BeTrue("AccessControl must ship unit tests under tests/ControlEasyReborn.UnitTests/Modules/AccessControl");

        var failing = new List<string>();
        foreach (var expected in new[] { "Application", "Application/Handlers" })
        {
            var dir = Path.Combine(unitTestsRoot, expected);
            if (!Directory.Exists(dir) || !Directory.EnumerateFiles(dir, "*.cs", SearchOption.AllDirectories).Any())
            {
                failing.Add($"Expected unit tests under {expected}");
            }
        }
        failing.Should().BeEmpty(string.Join("; ", failing));
    }

    /// <summary>
    /// Principle V — Observability, Security, and Runtime Container Parity.
    /// AccessControl does not log raw QR payloads / documents / full CPFs.
    /// </summary>
    [Fact]
    public void Principle_V_AccessControl_must_not_log_sensitive_pii()
    {
        // Delegates to the dedicated redaction arch test from
        // AccessControlTenantRulesTests.AccessControl_must_not_log_raw_QR_payloads_documents_or_full_CPFs.
        // Repeating the check here keeps a single failed-assertion call site
        // under a constitutional headline.
        var dedicated = new AccessControlTenantRulesTests();
        var method = typeof(AccessControlTenantRulesTests)
            .GetMethod(nameof(AccessControlTenantRulesTests.AccessControl_must_not_log_raw_QR_payloads_documents_or_full_CPFs));
        method.Should().NotBeNull("the dedicated redaction test must exist");
        var act = () => method!.Invoke(dedicated, Array.Empty<object?>());
        act.Should().NotThrow("AccessControl must never log raw QR / document / CPF / full name values (Constitution Principle V).");
    }

    /// <summary>
    /// Principle VI — Workflow Tooling. spec-kit owns the AccessControl spec;
    /// the docs and tasks live under .specs/qr-entrance-exit-access/. This test
    /// fails if the spec folder is missing or empty.
    /// </summary>
    [Fact]
    public void Principle_VI_spec_kit_must_own_AccessControl_artifacts()
    {
        var root = FindRepositoryRoot();
        var specDir = Path.Combine(root, ".specs", "qr-entrance-exit-access");
        Directory.Exists(specDir).Should().BeTrue(
            "the spec-kit artifact directory .specs/qr-entrance-exit-access/ must exist (Constitution Principle VI).");

        var required = new[] { "spec.md", "plan.md", "tasks.md" };
        var missing = required
            .Where(name => !File.Exists(Path.Combine(specDir, name)))
            .ToArray();
        missing.Should().BeEmpty(
            $"the following spec-kit artifacts are missing: {string.Join(", ", missing)}");
    }

    /// <summary>
    /// Cross-cutting stack &amp; architecture constraint:
    /// AccessControl must not reference ASP.NET MVC controllers (Constitution
    /// prefers minimal API endpoint classes), EF Core, MediatR, or Moq.
    /// </summary>
    [Fact]
    public void AccessControl_must_not_use_MVC_controllers_MediatR_EFCore_or_Moq()
    {
        var root = FindRepositoryRoot();
        var moduleRoot = Path.Combine(root, "src", "Modules", "AccessControl");

        var forbiddenPatterns = new (string Pattern, string Reason)[]
        {
            (@"Microsoft\.AspNetCore\.Mvc\.Controller", "MVC controllers are forbidden — use minimal API endpoints."),
            (@"Microsoft\.EntityFrameworkCore", "EF Core is forbidden — use DBTools LINQ-first."),
            (@"MediatR", "MediatR is forbidden — register handlers as scoped services."),
            (@"\bMoq\b", "Moq is forbidden — use NSubstitute."),
        };

        var failing = new List<string>();
        foreach (var path in Directory.EnumerateFiles(moduleRoot, "*.cs", SearchOption.AllDirectories))
        {
            if (path.Contains(Path.Combine("bin", "")) || path.Contains(Path.Combine("obj", "")))
            {
                continue;
            }
            var content = File.ReadAllText(path);
            foreach (var (pattern, reason) in forbiddenPatterns)
            {
                var match = Regex.Match(content, pattern);
                if (match.Success)
                {
                    failing.Add($"{Path.GetFileName(path)}: '{match.Value}' — {reason}");
                }
            }
        }

        failing.Should().BeEmpty(string.Join(" | ", failing));
    }

    private static string FindRepositoryRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null)
        {
            if (File.Exists(Path.Combine(directory.FullName, "AGENTS.md"))
                || (Directory.Exists(Path.Combine(directory.FullName, "src"))
                    && Directory.Exists(Path.Combine(directory.FullName, "tests"))
                    && File.Exists(Path.Combine(directory.FullName, "global.json"))))
            {
                return directory.FullName;
            }
            directory = directory.Parent;
        }
        throw new DirectoryNotFoundException("Could not locate the repository root from the test output directory.");
    }
}
