using ControlEasyReborn.Modules.Reports.Application.Abstractions;
using ControlEasyReborn.Modules.Reports.Application.Contracts;

namespace ControlEasyReborn.Modules.Reports.Application.Handlers;

public sealed class GetResidentsPerApartmentHandler
{
    private readonly IReportReadRepository _repository;

    public GetResidentsPerApartmentHandler(IReportReadRepository repository)
    {
        _repository = repository;
    }

    public Task<IReadOnlyList<ResidentsPerApartmentResponse>> HandleAsync(CancellationToken ct) =>
        _repository.GetResidentsPerApartmentAsync(ct);
}
