using ControlEasyReborn.Modules.Security.Application.Abstractions;
using ControlEasyReborn.Modules.Security.Application.Contracts;
using ControlEasyReborn.Modules.Security.Application.Errors;
using ControlEasyReborn.Modules.Security.Domain.Entities;
using FluentValidation;

namespace ControlEasyReborn.Modules.Security.Application.Handlers;

public sealed class CreateShiftHandler
{
    private readonly IShiftRepository _shifts;
    private readonly IValidator<CreateShiftRequest> _validator;

    public CreateShiftHandler(IShiftRepository shifts, IValidator<CreateShiftRequest> validator)
    {
        _shifts = shifts;
        _validator = validator;
    }

    public async Task<ShiftResponse> HandleAsync(CreateShiftRequest request, Guid tenantId, CancellationToken ct)
    {
        var result = await _validator.ValidateAsync(request, ct);
        if (!result.IsValid)
        {
            throw new Errors.ValidationException(result.Errors
                .GroupBy(e => e.PropertyName)
                .ToDictionary(g => g.Key, g => g.Select(e => e.ErrorMessage).ToArray()));
        }

        var shift = new Shift(
            id: Guid.NewGuid(),
            tenantId: tenantId,
            name: request.Name,
            startTime: request.StartTime,
            endTime: request.EndTime);

        await _shifts.AddAsync(shift, ct);
        return ToResponse(shift);
    }

    internal static ShiftResponse ToResponse(Shift s) =>
        new(s.Id, s.TenantId, s.Name, s.StartTime, s.EndTime, s.CrossesMidnight);
}