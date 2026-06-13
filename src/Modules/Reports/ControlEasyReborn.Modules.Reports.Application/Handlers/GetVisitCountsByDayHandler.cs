using ControlEasyReborn.Modules.Reports.Application.Abstractions;
using ControlEasyReborn.Modules.Reports.Application.Contracts;

namespace ControlEasyReborn.Modules.Reports.Application.Handlers;

public sealed class GetVisitCountsByDayHandler
{
    private readonly IReportReadRepository _repository;

    public GetVisitCountsByDayHandler(IReportReadRepository repository)
    {
        _repository = repository;
    }

    public Task<IReadOnlyList<VisitCountByDayResponse>> HandleAsync(
        DateOnly? from,
        DateOnly? to,
        CancellationToken ct) =>
        _repository.GetVisitCountsByDayAsync(from, to, ct);
}
