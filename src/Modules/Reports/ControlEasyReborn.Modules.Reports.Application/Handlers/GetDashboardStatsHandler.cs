using ControlEasyReborn.Modules.Reports.Application.Abstractions;
using ControlEasyReborn.Modules.Reports.Application.Contracts;

namespace ControlEasyReborn.Modules.Reports.Application.Handlers;

public sealed class GetDashboardStatsHandler
{
    private readonly IReportReadRepository _repository;

    public GetDashboardStatsHandler(IReportReadRepository repository)
    {
        _repository = repository;
    }

    public Task<DashboardStatsResponse> HandleAsync(CancellationToken ct) =>
        _repository.GetDashboardStatsAsync(ct);
}