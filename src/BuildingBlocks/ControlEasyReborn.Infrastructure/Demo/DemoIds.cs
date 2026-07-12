namespace ControlEasyReborn.Infrastructure.Demo;

public static class DemoIds
{
    public static readonly Guid PlatformTenantId = Guid.Parse("00000000-0000-0000-0000-000000000001");
    public static readonly Guid AuroraTenantId = Guid.Parse("00000000-0000-0000-0000-000000000010");
    public static readonly Guid ParqueVerdeTenantId = Guid.Parse("00000000-0000-0000-0000-000000000020");

    public static readonly Guid PlatformUserId = Guid.Parse("00000000-0000-0000-0000-000000000100");
    public static readonly Guid AdminUserId = Guid.Parse("00000000-0000-0000-0000-000000000101");
    public static readonly Guid PorteiroUserId = Guid.Parse("00000000-0000-0000-0000-000000000102");
    public static readonly Guid MoradorUserId = Guid.Parse("00000000-0000-0000-0000-000000000103");
    public static readonly Guid MultiUserId = Guid.Parse("00000000-0000-0000-0000-000000000104");

    public static readonly Guid AuroraShiftId = Guid.Parse("00000000-0000-0000-0000-000000000200");
    public static readonly Guid AuroraGatehouseId = Guid.Parse("00000000-0000-0000-0000-000000000201");
    public static readonly Guid ParqueShiftId = Guid.Parse("00000000-0000-0000-0000-000000000202");
    public static readonly Guid ParqueGatehouseId = Guid.Parse("00000000-0000-0000-0000-000000000203");

    public static readonly Guid PlatformProfileId = Guid.Parse("00000000-0000-0000-0000-000000000300");
    public static readonly Guid AdminProfileId = Guid.Parse("00000000-0000-0000-0000-000000000301");
    public static readonly Guid PorteiroProfileId = Guid.Parse("00000000-0000-0000-0000-000000000302");
    public static readonly Guid MoradorProfileId = Guid.Parse("00000000-0000-0000-0000-000000000303");
    public static readonly Guid MultiAuroraProfileId = Guid.Parse("00000000-0000-0000-0000-000000000304");
    public static readonly Guid MultiParqueProfileId = Guid.Parse("00000000-0000-0000-0000-000000000305");

    public const string DemoPassword = "demo123";

    public static Guid ApartmentId(Guid tenantId, string block, string apt) =>
        StableGuid($"apt:{tenantId:N}:{block}:{apt}");

    public static Guid ResidentId(Guid tenantId, int index) =>
        StableGuid($"resident:{tenantId:N}:{index}");

    public static Guid StableGuid(string name)
    {
        var bytes = System.Security.Cryptography.MD5.HashData(System.Text.Encoding.UTF8.GetBytes("controleasy-demo:" + name));
        return new Guid(bytes);
    }
}
