using System.Net;
using System.Net.Http.Json;
using ControlEasyReborn.Modules.Reports.Application.Contracts;
using ControlEasyReborn.Modules.Residents.Application.Contracts;
using ControlEasyReborn.Modules.Visits.Application.Contracts;
using FluentAssertions;
using Xunit;

namespace ControlEasyReborn.IntegrationTests;

[Collection("MySql Collection")]
public sealed class ReportEndpointTests
{
    private readonly TestcontainersWebApplicationFactory _factory;

    public ReportEndpointTests(MySqlContainerFixture mySql)
    {
        _factory = new TestcontainersWebApplicationFactory(mySql);
    }

    [Fact]
    public async Task GetVisitCountsByDay_ReturnsAggregatedRows()
    {
        var client = _factory.AsTenantA();

        await client.PostAsJsonAsync("/api/v1/visits",
            new CreateVisitRequest("Report Visitor", "11122233344", null, null, "Test"));

        var response = await client.GetAsync("/api/v1/reports/visit-counts-by-day");
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var body = await response.Content.ReadFromJsonAsync<List<VisitCountByDayResponse>>();
        body.Should().NotBeNull();
        body!.Should().NotBeEmpty();
    }

    [Fact]
    public async Task CrossTenant_GetResidentsPerApartment_AsDifferentTenant_ReturnsEmptyForForeignData()
    {
        var clientA = _factory.AsTenantA();
        var clientB = _factory.AsTenantB();
        var apartmentId = Guid.NewGuid();

        await clientA.PostAsJsonAsync("/api/v1/residents", new CreateResidentRequest(
            Name: "Apartment Resident",
            Cpf: "39053344705",
            Email: null,
            Phone: null,
            ApartmentId: apartmentId));

        var responseB = await clientB.GetAsync("/api/v1/reports/residents-per-apartment");
        responseB.StatusCode.Should().Be(HttpStatusCode.OK);

        var bodyB = await responseB.Content.ReadFromJsonAsync<List<ResidentsPerApartmentResponse>>();
        bodyB.Should().NotBeNull();
        bodyB!.Should().NotContain(x => x.ApartmentId == apartmentId);
    }
}
