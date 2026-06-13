using System.Reflection;
using FluentAssertions;
using Xunit;

namespace ControlEasyReborn.ArchitectureTests;

public sealed class TenantIdPropertyTests
{
    [Fact]
    public void Non_Platform_entities_in_Domain_Entities_must_have_non_nullable_TenantId()
    {
        var domainAssemblies = AppDomain.CurrentDomain.GetAssemblies()
            .Where(a => a.GetName().Name?.EndsWith(".Domain") == true
                        && a.GetName().Name?.StartsWith("ControlEasyReborn") == true)
            .ToArray();

        var entityTypes = domainAssemblies
            .SelectMany(a => a.GetTypes())
            .Where(t => t.Namespace?.EndsWith("Domain.Entities") == true
                        && t.IsClass && !t.IsAbstract)
            .ToList();

        var failing = new List<string>();

        foreach (var type in entityTypes)
        {
            if (type.Name.StartsWith("Platform") || type.Name == "User")
                continue;

            var tenantIdProp = type.GetProperty("TenantId", BindingFlags.Public | BindingFlags.Instance);

            if (tenantIdProp is null)
            {
                failing.Add($"{type.Name}: no TenantId property found");
                continue;
            }

            if (Nullable.GetUnderlyingType(tenantIdProp.PropertyType) is not null)
            {
                failing.Add($"{type.Name}: TenantId is nullable (Guid?), must be non-nullable Guid or TenantId value object");
                continue;
            }

            var propType = tenantIdProp.PropertyType;
            if (propType != typeof(Guid) && !IsTenantIdValueObject(propType))
            {
                failing.Add($"{type.Name}: TenantId type is {propType.Name}, expected Guid or TenantId value object wrapping Guid");
            }
        }

        failing.Should().BeEmpty(
            $"non-Platform entities must have a non-nullable TenantId property of type Guid or TenantId value object. Failures: {string.Join("; ", failing)}");
    }

    private static bool IsTenantIdValueObject(Type type)
    {
        if (type.Name != "TenantId") return false;
        var valueProp = type.GetProperty("Value", BindingFlags.Public | BindingFlags.Instance);
        return valueProp?.PropertyType == typeof(Guid);
    }
}