namespace ControlEasyReborn.Modules.Apartments.Application.Contracts;

public sealed record CreateApartmentRequest(string Block, string Unit);

public sealed record UpdateApartmentRequest(string Block, string Unit, bool Active);

public sealed record ApartmentResponse(
    Guid Id,
    Guid TenantId,
    string Block,
    string Unit,
    bool Active,
    DateTime CreatedAtUtc,
    DateTime? UpdatedAtUtc);
