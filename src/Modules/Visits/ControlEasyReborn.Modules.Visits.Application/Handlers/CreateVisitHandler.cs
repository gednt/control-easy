using ControlEasyReborn.Modules.Visits.Application.Abstractions;
using ControlEasyReborn.Modules.Visits.Application.Contracts;
using ControlEasyReborn.Modules.Visits.Application.Errors;
using ControlEasyReborn.Modules.Visits.Domain.Entities;
using FluentValidation;

namespace ControlEasyReborn.Modules.Visits.Application.Handlers;

public sealed class CreateVisitHandler
{
    private readonly IVisitRepository _visits;
    private readonly IValidator<CreateVisitRequest> _validator;

    public CreateVisitHandler(IVisitRepository visits, IValidator<CreateVisitRequest> validator)
    {
        _visits = visits;
        _validator = validator;
    }

    public async Task<VisitResponse> HandleAsync(CreateVisitRequest request, Guid tenantId, CancellationToken ct)
    {
        var result = await _validator.ValidateAsync(request, ct);
        if (!result.IsValid)
        {
            throw new Errors.ValidationException(result.Errors
                .GroupBy(e => e.PropertyName)
                .ToDictionary(g => g.Key, g => g.Select(e => e.ErrorMessage).ToArray()));
        }

        var visit = new Visit(
            id: Guid.NewGuid(),
            tenantId: tenantId,
            visitorName: request.VisitorName,
            visitorDocument: request.VisitorDocument,
            visitorPhone: request.VisitorPhone,
            apartmentId: request.ApartmentId,
            purpose: request.Purpose,
            status: VisitStatus.Pending,
            attendantProfileId: null,
            gatehouseId: null,
            checkedInAtUtc: null,
            checkedOutAtUtc: null,
            createdAtUtc: DateTime.UtcNow);

        await _visits.AddAsync(visit, ct);
        return ToResponse(visit);
    }

    internal static VisitResponse ToResponse(Visit v) =>
        new(v.Id, v.TenantId, v.VisitorName, v.VisitorDocument, v.VisitorPhone, v.ApartmentId, v.Purpose, v.Status.ToString(), v.AttendantProfileId, v.GatehouseId, v.CheckedInAtUtc, v.CheckedOutAtUtc, v.CreatedAtUtc, v.UpdatedAtUtc);
}