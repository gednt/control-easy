using FluentAssertions;
using NetArchTest.Rules;
using Xunit;

namespace ControlEasyReborn.ArchitectureTests;

public sealed class LayerDependencyTests
{
    [Fact]
    public void Domain_projects_should_not_reference_Infrastructure()
    {
        var domainAssemblies = AppDomain.CurrentDomain.GetAssemblies()
            .Where(a => a.GetName().Name?.EndsWith(".Domain") == true
                        && a.GetName().Name?.StartsWith("ControlEasyReborn") == true)
            .ToArray();

        var result = Types.InAssemblies(domainAssemblies)
            .ShouldNot()
            .HaveDependencyOnAny(
                "ControlEasyReborn.Modules.Residents.Infrastructure",
                "ControlEasyReborn.Modules.Tenants.Infrastructure",
                "ControlEasyReborn.Modules.Security.Infrastructure",
                "ControlEasyReborn.Infrastructure")
            .GetResult();

        result.IsSuccessful.Should().BeTrue();
    }

    [Fact]
    public void Infrastructure_projects_should_not_reference_EntityFrameworkCore_or_MySql_Data()
    {
        var infraAssemblies = AppDomain.CurrentDomain.GetAssemblies()
            .Where(a => a.GetName().Name?.EndsWith(".Infrastructure") == true
                        && a.GetName().Name?.StartsWith("ControlEasyReborn") == true)
            .ToArray();

        var result = Types.InAssemblies(infraAssemblies)
            .ShouldNot()
            .HaveDependencyOnAny(
                "Microsoft.EntityFrameworkCore",
                "MySql.Data")
            .GetResult();

        result.IsSuccessful.Should().BeTrue();
    }
}