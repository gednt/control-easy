using FluentAssertions;
using Xunit;

namespace ControlEasyReborn.ArchitectureTests;

public sealed class CrossTenantTestNamingTests
{
    [Fact]
    public void Integration_test_classes_touching_repositories_must_have_CrossTenant_fact()
    {
        var repositoryRoot = FindRepositoryRoot();
        var integrationDirectory = Path.Combine(repositoryRoot, "tests", "ControlEasyReborn.IntegrationTests");
        var failing = Directory.EnumerateFiles(integrationDirectory, "*EndpointTests.cs")
            .Where(path => !Path.GetFileName(path).StartsWith("Tenant", StringComparison.Ordinal))
            .Where(path => !System.Text.RegularExpressions.Regex.IsMatch(
                File.ReadAllText(path),
                @"\[Fact\][\s\S]*?Task\s+CrossTenant_[A-Za-z0-9_]+\s*\("))
            .Select(Path.GetFileName)
            .ToList();

        failing.Should().BeEmpty(
            $"the following integration test classes touch repositories but lack a [Fact] matching CrossTenant_.*: {string.Join(", ", failing)}");
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
