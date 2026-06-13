namespace ControlEasyReborn.Modules.ServiceProviders.Domain.Entities;

public sealed class ServiceProvider
{
    public Guid Id { get; private set; }
    public Guid TenantId { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public string Document { get; private set; } = string.Empty;
    public string? Phone { get; private set; }
    public string? Email { get; private set; }
    public string? ServiceType { get; private set; }
    public string? Company { get; private set; }
    public bool Active { get; private set; }
    public DateTime CreatedAtUtc { get; private set; }
    public DateTime? UpdatedAtUtc { get; private set; }

    private ServiceProvider() { }

    public ServiceProvider(Guid id, Guid tenantId, string name, string document, string? phone, string? email, string? serviceType, string? company, bool active, DateTime createdAtUtc, DateTime? updatedAtUtc = null)
    {
        Id = id;
        TenantId = tenantId;
        Name = name;
        Document = document;
        Phone = phone;
        Email = email;
        ServiceType = serviceType;
        Company = company;
        Active = active;
        CreatedAtUtc = createdAtUtc;
        UpdatedAtUtc = updatedAtUtc;
    }

    public void Deactivate()
    {
        Active = false;
        UpdatedAtUtc = DateTime.UtcNow;
    }

    public void UpdateDetails(string name, string document, string? phone, string? email, string? serviceType, string? company)
    {
        Name = name;
        Document = document;
        Phone = phone;
        Email = email;
        ServiceType = serviceType;
        Company = company;
        UpdatedAtUtc = DateTime.UtcNow;
    }
}