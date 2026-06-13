namespace ControlEasyReborn.Modules.Reports.Application.Contracts;

public sealed record VisitCountByDayResponse(DateOnly Date, int Count);

public sealed record ResidentsPerApartmentResponse(Guid? ApartmentId, int Count);
