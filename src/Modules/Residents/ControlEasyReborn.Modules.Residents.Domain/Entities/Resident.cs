namespace ControlEasyReborn.Modules.Residents.Domain.Entities;

public sealed class Resident
{
    public Guid Id { get; private set; }
    public Guid TenantId { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public string Cpf { get; private set; } = string.Empty;
    public string? Email { get; private set; }
    public string? Phone { get; private set; }
    public Guid? ApartmentId { get; private set; }
    public bool Active { get; private set; }
    public DateTime CreatedAtUtc { get; private set; }
    public DateTime? UpdatedAtUtc { get; private set; }

    private Resident() { }

    public Resident(Guid id, Guid tenantId, string name, string cpf, string? email, string? phone, Guid? apartmentId, bool active, DateTime createdAtUtc, DateTime? updatedAtUtc = null)
    {
        Id = id;
        TenantId = tenantId;
        Name = name;
        Cpf = cpf;
        Email = email;
        Phone = phone;
        ApartmentId = apartmentId;
        Active = active;
        CreatedAtUtc = createdAtUtc;
        UpdatedAtUtc = updatedAtUtc;
    }

    public void Deactivate()
    {
        Active = false;
        UpdatedAtUtc = DateTime.UtcNow;
    }

    public void UpdateDetails(string name, string cpf, string? email, string? phone, Guid? apartmentId)
    {
        Name = name;
        Cpf = cpf;
        Email = email;
        Phone = phone;
        ApartmentId = apartmentId;
        UpdatedAtUtc = DateTime.UtcNow;
    }
}