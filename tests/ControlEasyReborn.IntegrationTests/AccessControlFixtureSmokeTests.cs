using FluentAssertions;
using Xunit;

namespace ControlEasyReborn.IntegrationTests;

[Collection("AccessControl Collection")]
public sealed class AccessControlFixtureSmokeTests
{
    private readonly AccessControlFixture _accessControl;
    private readonly MySqlContainerFixture _mySql;

    public AccessControlFixtureSmokeTests(AccessControlFixture accessControl, MySqlContainerFixture mySql)
    {
        _accessControl = accessControl;
        _mySql = mySql;
    }

    private static Guid NewTenantId() => Guid.NewGuid();

    [Fact]
    public async Task SeedActiveApartment_persists_row()
    {
        var tenantId = NewTenantId();
        var block = "Z" + Guid.NewGuid().ToString("N")[..6];
        var unit = Guid.NewGuid().ToString("N")[..6];
        var apartmentId = await AccessControlFixture.SeedActiveApartmentAsync(_mySql.ConnectionString, tenantId, block, unit);

        await using var conn = new MySqlConnector.MySqlConnection(_mySql.ConnectionString);
        await conn.OpenAsync();
        await using var cmd = new MySqlConnector.MySqlCommand(
            "SELECT Block, Unit, Active FROM Apartments WHERE Id = @id", conn);
        cmd.Parameters.AddWithValue("@id", apartmentId.ToString());
        await using var reader = await cmd.ExecuteReaderAsync();
        var found = await reader.ReadAsync();
        found.Should().BeTrue();
        reader.GetString(0).Should().Be(block);
        reader.GetString(1).Should().Be(unit);
        reader.GetBoolean(2).Should().BeTrue();
    }

    [Fact]
    public async Task SeedActiveResident_persists_row()
    {
        var tenantId = NewTenantId();
        var apartmentId = await AccessControlFixture.SeedActiveApartmentAsync(_mySql.ConnectionString, tenantId, "Z" + Guid.NewGuid().ToString("N")[..6], Guid.NewGuid().ToString("N")[..6]);
        var residentId = await AccessControlFixture.SeedActiveResidentAsync(_mySql.ConnectionString, tenantId, "Maria", Guid.NewGuid().ToString("N")[..11], apartmentId);

        await using var conn = new MySqlConnector.MySqlConnection(_mySql.ConnectionString);
        await conn.OpenAsync();
        await using var cmd = new MySqlConnector.MySqlCommand(
            "SELECT Name, Active, ApartmentId FROM Residents WHERE Id = @id", conn);
        cmd.Parameters.AddWithValue("@id", residentId.ToString());
        await using var reader = await cmd.ExecuteReaderAsync();
        var found = await reader.ReadAsync();
        found.Should().BeTrue();
        reader.GetString(0).Should().Be("Maria");
        reader.GetBoolean(1).Should().BeTrue();
        reader.GetGuid(2).Should().Be(apartmentId);
    }

    [Fact]
    public async Task SeedActiveVehicle_persists_row_with_owner()
    {
        var tenantId = NewTenantId();
        var apartmentId = await AccessControlFixture.SeedActiveApartmentAsync(_mySql.ConnectionString, tenantId, "Z" + Guid.NewGuid().ToString("N")[..6], Guid.NewGuid().ToString("N")[..6]);
        var residentId = await AccessControlFixture.SeedActiveResidentAsync(_mySql.ConnectionString, tenantId, "Owner", Guid.NewGuid().ToString("N")[..11], apartmentId);
        var plate = "Z" + Guid.NewGuid().ToString("N")[..6].ToUpperInvariant();
        var vehicleId = await AccessControlFixture.SeedActiveVehicleAsync(_mySql.ConnectionString, tenantId, plate, apartmentId, residentId);

        await using var conn = new MySqlConnector.MySqlConnection(_mySql.ConnectionString);
        await conn.OpenAsync();
        await using var cmd = new MySqlConnector.MySqlCommand(
            "SELECT Plate, Active, OwnerResidentId FROM Vehicles WHERE Id = @id", conn);
        cmd.Parameters.AddWithValue("@id", vehicleId.ToString());
        await using var reader = await cmd.ExecuteReaderAsync();
        var found = await reader.ReadAsync();
        found.Should().BeTrue();
        reader.GetString(0).Should().Be(plate);
        reader.GetBoolean(1).Should().BeTrue();
        reader.GetGuid(2).Should().Be(residentId);
    }

    [Fact]
    public async Task SeedActiveCredential_persists_row()
    {
        var tenantId = NewTenantId();
        var residentId = await AccessControlFixture.SeedActiveResidentAsync(_mySql.ConnectionString, tenantId, "Crd", Guid.NewGuid().ToString("N")[..11], null);
        var actorId = Guid.NewGuid();
        var verifier = new byte[32];
        new Random(42).NextBytes(verifier);
        var credentialId = await AccessControlFixture.SeedActiveCredentialAsync(
            _mySql.ConnectionString,
            tenantId,
            ControlEasyReborn.Modules.AccessControl.Domain.ValueObjects.SubjectType.Resident,
            residentId,
            verifier,
            keyVersion: 1,
            issuedByProfileId: actorId);

        await using var conn = new MySqlConnector.MySqlConnection(_mySql.ConnectionString);
        await conn.OpenAsync();
        await using var cmd = new MySqlConnector.MySqlCommand(
            "SELECT SubjectType, Status, KeyVersion FROM AccessCredentials WHERE Id = @id", conn);
        cmd.Parameters.AddWithValue("@id", credentialId.ToString());
        await using var reader = await cmd.ExecuteReaderAsync();
        var found = await reader.ReadAsync();
        found.Should().BeTrue();
        reader.GetInt32(0).Should().Be((int)ControlEasyReborn.Modules.AccessControl.Domain.ValueObjects.SubjectType.Resident);
        reader.GetInt32(1).Should().Be((int)ControlEasyReborn.Modules.AccessControl.Domain.ValueObjects.CredentialStatus.Active);
        reader.GetInt32(2).Should().Be(1);
    }

    [Fact]
    public async Task CleanupAccessControl_removes_only_target_tenant_rows()
    {
        var tenantA = NewTenantId();
        var tenantB = NewTenantId();

        await AccessControlFixture.CleanupAccessControlAsync(_mySql.ConnectionString, tenantA);
        await AccessControlFixture.CleanupAccessControlAsync(_mySql.ConnectionString, tenantB);

        var residentB = await AccessControlFixture.SeedActiveResidentAsync(_mySql.ConnectionString, tenantB, "Tenant B Resident", Guid.NewGuid().ToString("N")[..11], null);
        var actor = Guid.NewGuid();
        await AccessControlFixture.SeedActiveCredentialAsync(_mySql.ConnectionString, tenantB, ControlEasyReborn.Modules.AccessControl.Domain.ValueObjects.SubjectType.Resident, residentB, new byte[32], 1, actor);

        await using var conn = new MySqlConnector.MySqlConnection(_mySql.ConnectionString);
        await conn.OpenAsync();
        await using var cmd = new MySqlConnector.MySqlCommand(
            "SELECT COUNT(*) FROM AccessCredentials WHERE tenant_id = @tid", conn);
        cmd.Parameters.AddWithValue("@tid", tenantB.ToString());
        var beforeCleanupTenantB = Convert.ToInt32(await cmd.ExecuteScalarAsync());
        beforeCleanupTenantB.Should().Be(1);

        await AccessControlFixture.CleanupAccessControlAsync(_mySql.ConnectionString, tenantA);

        await using var cmd2 = new MySqlConnector.MySqlCommand(
            "SELECT COUNT(*) FROM AccessCredentials WHERE tenant_id = @tid", conn);
        cmd2.Parameters.AddWithValue("@tid", tenantB.ToString());
        var remainingB = Convert.ToInt32(await cmd2.ExecuteScalarAsync());
        remainingB.Should().Be(1, "cleanup targeting tenant A must not touch tenant B's credentials");
    }
}
