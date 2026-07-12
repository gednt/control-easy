using System.Reflection;
using FluentAssertions;
using Xunit;

namespace ControlEasyReborn.ArchitectureTests;

public sealed class CrossTenantTestNamingTests
{
    [Fact]
    public void Integration_test_classes_touching_repositories_must_have_CrossTenant_fact()
    {
        var testAssembly = LoadIntegrationTestAssembly();
        if (testAssembly is null)
            return;

        var testClasses = testAssembly.GetTypes()
            .Where(t => t.GetMethods().Any(m => m.GetCustomAttributes(false).Any(a => a.GetType().Name == "FactAttribute")))
            .ToList();

        var failing = new List<string>();

        foreach (var testClass in testClasses)
        {
            if (!TouchesRepository(testClass))
                continue;

            var hasCrossTenant = testClass.GetMethods()
                .Any(m => m.GetCustomAttributes(false)
                    .Any(a => a.GetType().Name == "FactAttribute") &&
                    System.Text.RegularExpressions.Regex.IsMatch(m.Name, @"CrossTenant_.*"));

            if (!hasCrossTenant)
            {
                failing.Add(testClass.Name);
            }
        }

        failing.Should().BeEmpty(
            $"the following integration test classes touch repositories but lack a [Fact] matching CrossTenant_.*: {string.Join(", ", failing)}");
    }

    private static Assembly? LoadIntegrationTestAssembly()
    {
        return AppDomain.CurrentDomain.GetAssemblies()
            .FirstOrDefault(a => a.GetName().Name == "ControlEasyReborn.IntegrationTests");
    }

    private static bool TouchesRepository(Type testClass)
    {
        var ctors = testClass.GetConstructors();
        foreach (var ctor in ctors)
        {
            foreach (var param in ctor.GetParameters())
            {
                if (param.ParameterType.Name.Contains("Repository") ||
                    param.ParameterType.Name.Contains("Factory") ||
                    param.ParameterType.Name.Contains("TestFixture"))
                {
                    return true;
                }
            }
        }

        return testClass.GetFields(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance)
            .Any(f => f.FieldType.Name.Contains("Factory") || f.FieldType.Name.Contains("Repository"));
    }
}