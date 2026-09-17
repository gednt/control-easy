using System.Net;
using System.Net.Http.Json;
using ControlEasyReborn.Modules.Apartments.Application.Contracts;
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
        var apartmentId = await _factory.SeedApartmentForTenantAAsync();

        var createResponse = await client.PostAsJsonAsync("/api/v1/visits",
            new CreateVisitRequest("Report Visitor", "11122233344", null, apartmentId, "Test"));
        createResponse.EnsureSuccessStatusCode();

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

    [Fact]
    public async Task DashboardStats_OccupiedApartments_IsTenantScoped()
    {
        var clientA = _factory.AsTenantA();
        var clientB = _factory.AsTenantB();
        var suffix = Guid.NewGuid().ToString("N")[..8];

        var beforeA = await clientA.GetFromJsonAsync<DashboardStatsResponse>("/api/v1/dashboard/stats");
        var beforeB = await clientB.GetFromJsonAsync<DashboardStatsResponse>("/api/v1/dashboard/stats");

        var apartmentResponse = await clientA.PostAsJsonAsync(
            "/api/v1/apartments",
            new CreateApartmentRequest($"T{suffix[..3]}", suffix[3..]));
        apartmentResponse.StatusCode.Should().Be(HttpStatusCode.Created);
        var apartment = await apartmentResponse.Content.ReadFromJsonAsync<ApartmentResponse>();

        var residentResponse = await clientA.PostAsJsonAsync(
            "/api/v1/residents",
            new CreateResidentRequest(
                Name: $"Dashboard Resident {suffix}",
                Cpf: GenerateCpf(suffix),
                Email: null,
                Phone: null,
                ApartmentId: apartment!.Id));
        residentResponse.StatusCode.Should().Be(HttpStatusCode.Created);

        var afterA = await clientA.GetFromJsonAsync<DashboardStatsResponse>("/api/v1/dashboard/stats");
        var afterB = await clientB.GetFromJsonAsync<DashboardStatsResponse>("/api/v1/dashboard/stats");

        afterA!.OccupiedApartments.Should().Be(beforeA!.OccupiedApartments + 1);
        afterB!.OccupiedApartments.Should().Be(beforeB!.OccupiedApartments);
    }

    private static string GenerateCpf(string suffix)
    {
        var seed = Math.Abs(suffix.GetHashCode()).ToString("D9")[..9];
        var digits = seed.Select(c => c - '0').ToArray();
        var first = CalculateCpfDigit(digits, 10);
        var second = CalculateCpfDigit(digits.Append(first).ToArray(), 11);
        return seed + first + second;
    }

    private static int CalculateCpfDigit(IReadOnlyList<int> digits, int weight)
    {
        var sum = digits.Select((digit, index) => digit * (weight - index)).Sum();
        var remainder = sum % 11;
        return remainder < 2 ? 0 : 11 - remainder;
    }
}
