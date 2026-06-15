namespace ControlEasyReborn.Modules.Reports.Application.Contracts;

public sealed record VisitCountByDayResponse(DateOnly Date, int Count);

public sealed record ResidentsPerApartmentResponse(Guid? ApartmentId, int Count);

public sealed record DashboardStatsResponse(
    int TotalResidents,
    int ActiveResidents,
    int TotalVehicles,
    int ActiveVehicles,
    int TotalApartments,
    int OccupiedApartments,
    int OpenVisits,
    int TodayVisits,
    List<RecentVisitDto> RecentVisits);

public sealed record RecentVisitDto(
    Guid Id,
    string VisitorName,
    string? Purpose,
    string Status,
    string? ApartmentLabel,
    DateTime CreatedAtUtc);
