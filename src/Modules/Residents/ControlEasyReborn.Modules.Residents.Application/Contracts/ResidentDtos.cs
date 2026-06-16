namespace ControlEasyReborn.Modules.Residents.Application.Contracts;

public sealed record CreateResidentRequest(string Name, string Cpf, string? Email, string? Phone, Guid? ApartmentId);

public sealed record UpdateResidentRequest(string Name, string Cpf, string? Email, string? Phone, Guid? ApartmentId, bool Active = true);

public sealed record ResidentResponse(
    Guid Id,
    Guid TenantId,
    string Name,
    string Cpf,
    string? Email,
    string? Phone,
    Guid? ApartmentId,
    bool Active,
    DateTime CreatedAtUtc);