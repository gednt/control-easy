using System.Reflection;
using System.Text.RegularExpressions;
using FluentAssertions;
using Xunit;

namespace ControlEasyReborn.ArchitectureTests;

public sealed class AccessControlBiometricExclusionTests
{
    private static readonly string[] AccessControlAssemblyNames = new[]
    {
        "ControlEasyReborn.Modules.AccessControl.Domain",
        "ControlEasyReborn.Modules.AccessControl.Application",
        "ControlEasyReborn.Modules.AccessControl.Infrastructure",
        "ControlEasyReborn.Modules.AccessControl.Api"
    };

    private static readonly Regex BiometricCodeRegex = new(
        @"\b(?:facial_biometric|biometric_template|biometric_template_id|embedding|liveness_score|touchless_face|face_match)\b",
        RegexOptions.IgnoreCase);

    static AccessControlBiometricExclusionTests()
    {
        EnsureAccessControlAssembliesLoaded();
    }

    [Fact]
    public void AccessControl_code_must_not_define_biometric_services_or_fields()
    {
        var accessControlAssemblies = AppDomain.CurrentDomain.GetAssemblies()
            .Where(a => AccessControlAssemblyNames.Contains(a.GetName().Name ?? string.Empty, StringComparer.Ordinal))
            .ToArray();

        accessControlAssemblies.Should().NotBeEmpty();

        var failing = new List<string>();
        foreach (var assembly in accessControlAssemblies)
        {
            foreach (var type in assembly.GetTypes())
            {
                if (type.IsNestedPrivate && !type.IsPublic) continue;

                foreach (var name in new[] { type.Name, type.FullName })
                {
                    if (name is null) continue;
                    if (BiometricCodeRegex.IsMatch(name))
                    {
                        failing.Add($"{type.FullName}: type name contains biometric keyword");
                    }
                }

                foreach (var field in type.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance))
                {
                    CheckStringMember(field, type, failing);
                }
                foreach (var property in type.GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance))
                {
                    CheckStringMember(property, type, failing);
                }
            }
        }

        failing.Should().BeEmpty(
            "biometric keyword (facial_biometric, biometric_template, embedding, liveness_score, face_match) is reserved for a future release and must not appear in compiled AccessControl code. Failures: "
            + string.Join("; ", failing));
    }

    [Fact]
    public void AccessControl_must_not_expose_biometric_routes_or_OpenAPI_schemas()
    {
        var accessControlAssemblies = AppDomain.CurrentDomain.GetAssemblies()
            .Where(a => AccessControlAssemblyNames.Contains(a.GetName().Name ?? string.Empty, StringComparer.Ordinal))
            .ToArray();

        accessControlAssemblies.Should().NotBeEmpty();

        var failing = new List<string>();
        foreach (var assembly in accessControlAssemblies)
        {
            foreach (var type in assembly.GetTypes())
            {
                foreach (var method in type.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance | BindingFlags.DeclaredOnly))
                {
                    if (BiometricCodeRegex.IsMatch(method.Name))
                    {
                        failing.Add($"{type.FullName}.{method.Name}: method name contains biometric keyword");
                    }
                }
            }
        }

        failing.Should().BeEmpty(
            "biometric methods or routes must not exist in the AccessControl module. Failures: "
            + string.Join("; ", failing));
    }

    private static void CheckStringMember(MemberInfo member, Type type, List<string> failing)
    {
        object? value = member switch
        {
            FieldInfo f when f.IsLiteral && !f.IsInitOnly => f.GetRawConstantValue(),
            PropertyInfo p when p.GetIndexParameters().Length == 0 && p.CanRead => SafeGetValue(p),
            _ => null
        };
        if (value is not string s) return;
        if (BiometricCodeRegex.IsMatch(s))
        {
            failing.Add($"{type.FullName}.{member.Name}: contains biometric keyword in value '{s}'");
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
                try { Assembly.LoadFrom(candidate); } catch { }
            }
        }
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