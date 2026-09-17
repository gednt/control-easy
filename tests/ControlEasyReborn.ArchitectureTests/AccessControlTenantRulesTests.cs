using System.Reflection;
using System.Text.RegularExpressions;
using FluentAssertions;
using NetArchTest.Rules;
using Xunit;

namespace ControlEasyReborn.ArchitectureTests;

public sealed class AccessControlTenantRulesTests
{
    private static readonly string[] AccessControlAssemblyNames = new[]
    {
        "ControlEasyReborn.Modules.AccessControl.Domain",
        "ControlEasyReborn.Modules.AccessControl.Application",
        "ControlEasyReborn.Modules.AccessControl.Infrastructure",
        "ControlEasyReborn.Modules.AccessControl.Api"
    };

    static AccessControlTenantRulesTests()
    {
        EnsureAccessControlAssembliesLoaded();
    }

    private static void EnsureAccessControlAssembliesLoaded()
    {
        var loaded = AppDomain.CurrentDomain.GetAssemblies()
            .Select(a => a.GetName().Name ?? string.Empty)
            .ToHashSet(StringComparer.Ordinal);

        var root = FindRepositoryRoot();
        var candidates = Directory.EnumerateFiles(
                Path.Combine(root, "src", "Modules", "AccessControl"),
                "*.dll",
                SearchOption.AllDirectories)
            .Where(p => p.Contains(Path.DirectorySeparatorChar + "bin" + Path.DirectorySeparatorChar, StringComparison.Ordinal))
            .Where(p => p.EndsWith(".dll", StringComparison.Ordinal))
            .ToArray();

        foreach (var candidate in candidates)
        {
            var name = Path.GetFileNameWithoutExtension(candidate);
            if (name.StartsWith("ControlEasyReborn.Modules.AccessControl", StringComparison.Ordinal)
                && !loaded.Contains(name))
            {
                try
                {
                    Assembly.LoadFrom(candidate);
                }
                catch
                {
                    // Ignore load failures; the test will fail with a clear message below.
                }
            }
        }
    }

    [Fact]
    public void AccessControl_entities_must_have_non_nullable_TenantId_property()
    {
        var accessControlDomain = AppDomain.CurrentDomain.GetAssemblies()
            .FirstOrDefault(a => a.GetName().Name == "ControlEasyReborn.Modules.AccessControl.Domain");
        accessControlDomain.Should().NotBeNull("AccessControl.Domain must be loaded by the test host");

        var entityTypes = accessControlDomain!
            .GetTypes()
            .Where(t => t.Namespace?.EndsWith("Domain.Entities") == true
                        && t.IsClass && !t.IsAbstract)
            .ToList();

        entityTypes.Should().NotBeEmpty("AccessControl.Domain.Entities should contain the new access-control entities");

        var failing = new List<string>();
        foreach (var type in entityTypes)
        {
            var tenantIdProp = type.GetProperty("TenantId", BindingFlags.Public | BindingFlags.Instance);
            if (tenantIdProp is null)
            {
                failing.Add($"{type.FullName}: missing TenantId property");
                continue;
            }
            if (Nullable.GetUnderlyingType(tenantIdProp.PropertyType) is not null)
            {
                failing.Add($"{type.FullName}: TenantId is nullable (must be non-nullable Guid)");
                continue;
            }
            if (tenantIdProp.PropertyType != typeof(Guid))
            {
                failing.Add($"{type.FullName}: TenantId type is {tenantIdProp.PropertyType.Name} (expected Guid)");
            }
        }

        failing.Should().BeEmpty(
            $"every AccessControl entity must carry a non-nullable Guid TenantId property. Failures: {string.Join("; ", failing)}");
    }

    [Fact]
    public void AccessControl_assemblies_must_not_reference_MySql_Data()
    {
        var accessControlAssemblies = AppDomain.CurrentDomain.GetAssemblies()
            .Where(a => AccessControlAssemblyNames.Contains(a.GetName().Name ?? string.Empty, StringComparer.Ordinal))
            .ToArray();

        accessControlAssemblies.Should().NotBeEmpty();

        var result = Types.InAssemblies(accessControlAssemblies)
            .ShouldNot()
            .HaveDependencyOn("MySql.Data")
            .GetResult();

        result.IsSuccessful.Should().BeTrue(
            "the AccessControl module uses DBTools LINQ-first; raw MySql.Data usage is forbidden (FR-013, Constitution III)");
    }

    [Fact]
    public void AccessControl_must_not_contain_raw_SQL_strings_targeting_other_module_tables()
    {
        var accessControlAssemblies = AppDomain.CurrentDomain.GetAssemblies()
            .Where(a => AccessControlAssemblyNames.Contains(a.GetName().Name ?? string.Empty, StringComparer.Ordinal))
            .ToArray();

        accessControlAssemblies.Should().NotBeEmpty();

        // Constitution III: AccessControl must not bypass owner-module ports by writing
        // raw SQL that touches owner tables (Residents, Vehicles, Apartments, Visits, etc.).
        var ownerTables = new[] { "Residents", "Vehicles", "Apartments", "Visits", "Users", "Photos", "ConsentAuditLog", "ServiceProviders" };
        var failing = new List<string>();
        foreach (var assembly in accessControlAssemblies)
        {
            foreach (var type in assembly.GetTypes())
            {
                foreach (var field in type.GetFields(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance))
                {
                    CheckStringValue(field, field.Name, type.FullName, ownerTables, failing);
                }
                foreach (var property in type.GetProperties(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance))
                {
                    CheckStringValue(property, property.Name, type.FullName, ownerTables, failing);
                }
            }
        }

        failing.Should().BeEmpty(
            $"AccessControl must not reference owner-module tables directly. Use directory ports (IResidentDirectory, IVehicleDirectory, IApartmentDirectory). Failures: {string.Join("; ", failing)}");
    }

    private static void CheckStringValue(MemberInfo member, string memberName, string? typeFullName, string[] ownerTables, List<string> failing)
    {
        object? value = member switch
        {
            FieldInfo f => f.IsLiteral && !f.IsInitOnly ? f.GetRawConstantValue() : null,
            PropertyInfo p when p.GetIndexParameters().Length == 0 && p.CanRead => SafeGetValue(p),
            _ => null
        };
        if (value is not string s) return;
        foreach (var owner in ownerTables)
        {
            // Allow type-safe references like "Resident" (SubjectType enum value) but block
            // string literals that look like SQL table names embedded in WHERE clauses.
            if (Regex.IsMatch(s, $@"\b(?:FROM|INTO|UPDATE|JOIN)\s+`?{owner}`?\b", RegexOptions.IgnoreCase)
                || Regex.IsMatch(s, $@"^\s*{owner}\s*$", RegexOptions.IgnoreCase))
            {
                failing.Add($"{typeFullName}.{memberName} references owner table '{owner}'");
            }
        }
    }

    private static object? SafeGetValue(PropertyInfo p)
    {
        try
        {
            return p.GetValue(null);
        }
        catch
        {
            return null;
        }
    }

    [Fact]
    public void AccessControl_must_not_introduce_biometric_columns_or_strings()
    {
        var root = FindRepositoryRoot();
        var sqlFiles = Directory.EnumerateFiles(
                Path.Combine(root, "docker", "mysql"),
                "*.sql", SearchOption.AllDirectories)
            .ToArray();

        sqlFiles.Should().NotBeEmpty("the project must ship its init and migration SQL under docker/mysql");

        var forbiddenColumnRegex = new Regex(
            @"\b(?:facial_biometric|biometric_template|biometric_template_id|embedding|liveness_score)\b",
            RegexOptions.IgnoreCase);

        var failing = new List<string>();
        foreach (var path in sqlFiles)
        {
            var content = File.ReadAllText(path);
            // Reserved vocabulary allowed inside a comment that explicitly says it is reserved
            // for a future release (e.g. "-- reserved", "-- future"). Otherwise any occurrence
            // of a biometric column name in DDL is a regression.
            foreach (Match match in forbiddenColumnRegex.Matches(content))
            {
                var lineStart = content.LastIndexOf('\n', Math.Max(0, match.Index - 1)) + 1;
                var lineEnd = content.IndexOf('\n', match.Index);
                if (lineEnd < 0) lineEnd = content.Length;
                var line = content.Substring(lineStart, lineEnd - lineStart).TrimStart();
                if (Regex.IsMatch(line, @"^\s*--") || Regex.IsMatch(line, @"\breserved\b", RegexOptions.IgnoreCase))
                {
                    continue;
                }
                failing.Add($"{Path.GetFileName(path)}: biometric symbol '{match.Value}' in line: {line}");
            }
        }

        failing.Should().BeEmpty(
            $"facial_biometric / biometric_template / embedding / liveness_score must not appear in any active DDL (US6). Failures: {string.Join("; ", failing)}");
    }

    [Fact]
    public void AccessControl_must_not_log_raw_QR_payloads_documents_or_full_CPFs()
    {
        var root = FindRepositoryRoot();
        var accessControlRoot = Path.Combine(root, "src", "Modules", "AccessControl");
        Directory.Exists(accessControlRoot).Should().BeTrue("the AccessControl module is expected under src/Modules/AccessControl");

        var sensitiveNames = new[]
        {
            "QrPayload",
            "RawPayload",
            "SecretVerifier",
            "DocumentNumber",
            "DocumentValue",
            "FullName",
            "Cpf",
            "Plate"
        };

        var logCallRegex = new Regex(
            @"\.(?:Log(?:Debug|Information|Warning|Error|Fatal|Trace))(?:\s*<[^>]*>)?\s*\(",
            RegexOptions.IgnoreCase);

        var failing = new List<string>();
        foreach (var path in Directory.EnumerateFiles(accessControlRoot, "*.cs", SearchOption.AllDirectories))
        {
            var content = File.ReadAllText(path);
            foreach (Match call in logCallRegex.Matches(content))
            {
                var openParen = content.IndexOf(')', call.Index);
                var closeFromOpen = content.IndexOf(')', Math.Max(call.Index, call.Index + call.Length));
                int closeIndex = openParen;
                if (closeIndex < 0)
                {
                    continue;
                }
                if (closeFromOpen > 0 && closeFromOpen < closeIndex)
                {
                    closeIndex = closeFromOpen;
                }
                var callSpan = content.Substring(call.Index, Math.Min(closeIndex - call.Index + 1, content.Length - call.Index));
                foreach (var sensitive in sensitiveNames)
                {
                    if (callSpan.IndexOf(sensitive, StringComparison.OrdinalIgnoreCase) >= 0)
                    {
                        failing.Add($"{Path.GetFileName(path)}: log call exposes sensitive token '{sensitive}' at offset {call.Index}.");
                    }
                }
            }
        }

        failing.Should().BeEmpty(
            $"AccessControl log calls must not pass raw QR, document, CPF, or full name values. Failures: {string.Join("; ", failing)}");
    }

    private static string FindRepositoryRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null)
        {
            if (File.Exists(Path.Combine(directory.FullName, "AGENTS.md")))
                return directory.FullName;
            directory = directory.Parent;
        }
        throw new DirectoryNotFoundException("Could not locate the repository root from the test output directory.");
    }
}

