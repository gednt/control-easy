using System.Net;
using System.Net.Http.Json;
using ControlEasyReborn.Modules.Visits.Application.Contracts;
using FluentAssertions;
using Xunit;

namespace ControlEasyReborn.IntegrationTests;

[Collection("MySql Collection")]
public sealed class VisitDestinationValidationTests
{
    private readonly MySqlContainerFixture _mySql;

    public VisitDestinationValidationTests(MySqlContainerFixture mySql)
    {
        _mySql = mySql;
    }

    private TestcontainersWebApplicationFactory NewFactory() => new(_mySql);

    private static Guid NewTenantId() => Guid.NewGuid();

    [Fact]
    public async Task CreateVisit_with_missing_apartment_returns_400_validation()
    {
        var tenantId = NewTenantId();
        var factory = NewFactory();
        var client = VisitDestinationTestClient.ForTenant(factory, tenantId);

        var request = new CreateVisitRequest("Visit Missing Apt", "12345678901", null, ApartmentId: null, Purpose: null);
        var response = await client.PostAsJsonAsync("/api/v1/visits", request);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task CreateVisit_with_inactive_apartment_returns_400_validation()
    {
        var tenantId = NewTenantId();
        var apartmentId = Guid.NewGuid();
        await AccessControlFixture.SeedActiveApartmentAsync(_mySql.ConnectionString, tenantId, "BLOCK", Guid.NewGuid().ToString("N")[..6], ct: default);
        await using (var conn = new MySqlConnector.MySqlConnection(_mySql.ConnectionString))
        {
            await conn.OpenAsync();
            await using var cmd = new MySqlConnector.MySqlCommand(
                "INSERT INTO Apartments (Id, TenantId, Block, Unit, Active, CreatedAtUtc, tenant_id) " +
                "VALUES (@id, @tid, 'B', 'U', 0, UTC_TIMESTAMP(6), @tid)", conn);
            cmd.Parameters.AddWithValue("@id", apartmentId.ToString());
            cmd.Parameters.AddWithValue("@tid", tenantId.ToString());
            await cmd.ExecuteNonQueryAsync();
        }

        var factory = NewFactory();
        var client = VisitDestinationTestClient.ForTenant(factory, tenantId);

        var request = new CreateVisitRequest("Visit Inactive Apt", "12345678901", null, apartmentId, null);
        var response = await client.PostAsJsonAsync("/api/v1/visits", request);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task CreateVisit_with_active_apartment_persists_block_and_unit_snapshot()
    {
        var tenantId = NewTenantId();
        var block = "BLOCK-" + Guid.NewGuid().ToString("N")[..6];
        var unit = Guid.NewGuid().ToString("N")[..6];
        var apartmentId = await AccessControlFixture.SeedActiveApartmentAsync(_mySql.ConnectionString, tenantId, block, unit);

        var factory = NewFactory();
        var client = VisitDestinationTestClient.ForTenant(factory, tenantId);

        var request = new CreateVisitRequest("Visit Snapshot", "12345678901", null, apartmentId, null);
        var response = await client.PostAsJsonAsync("/api/v1/visits", request);

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var body = await response.Content.ReadFromJsonAsync<VisitResponse>();
        body.Should().NotBeNull();
        body!.ApartmentId.Should().Be(apartmentId);
        body.DestinationBlock.Should().Be(block);
        body.DestinationUnit.Should().Be(unit);
    }

    private static class VisitDestinationTestClient
    {
        public static HttpClient ForTenant(TestcontainersWebApplicationFactory factory, Guid tenantId)
        {
            var token = JwtTestHelper.GenerateTenantToken(
                userId: Guid.NewGuid(),
                email: $"visit-dest-{tenantId}@controleasy.local",
                tenantId: tenantId,
                roles: new[] { "GatehouseOperator" },
                permissions: new[] { "Visits.Create", "Visits.Read" });
            var client = factory.CreateClient();
            client.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            return client;
        }
    }
}