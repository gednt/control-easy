namespace ControlEasyReborn.Modules.Residents.Domain.Entities;

public sealed class ResidentIdentityDocument
{
    public Guid Id { get; private set; }
    public Guid TenantId { get; private set; }
    public Guid ResidentId { get; private set; }
    public string DocumentType { get; private set; } = string.Empty;
    public string NormalizedValue { get; private set; } = string.Empty;
    public bool Active { get; private set; }
    public DateTime CreatedAtUtc { get; private set; }
    public DateTime? UpdatedAtUtc { get; private set; }

    private ResidentIdentityDocument() { }

    public ResidentIdentityDocument(Guid id, Guid tenantId, Guid residentId, string documentType, string normalizedValue, bool active, DateTime createdAtUtc, DateTime? updatedAtUtc = null)
    {
        if (residentId == Guid.Empty) throw new InvalidOperationException("Resident id is required.");
        if (string.IsNullOrWhiteSpace(documentType)) throw new InvalidOperationException("Document type is required.");
        if (documentType.Length > 32) throw new InvalidOperationException("Document type exceeds 32 characters.");
        if (string.IsNullOrWhiteSpace(normalizedValue)) throw new InvalidOperationException("Normalized value is required.");
        if (normalizedValue.Length > 64) throw new InvalidOperationException("Normalized value exceeds 64 characters.");

        Id = id;
        TenantId = tenantId;
        ResidentId = residentId;
        DocumentType = documentType;
        NormalizedValue = normalizedValue;
        Active = active;
        CreatedAtUtc = createdAtUtc;
        UpdatedAtUtc = updatedAtUtc;
    }

    public void Deactivate()
    {
        Active = false;
        UpdatedAtUtc = DateTime.UtcNow;
    }

    public void UpdateValue(string normalizedValue)
    {
        if (string.IsNullOrWhiteSpace(normalizedValue)) throw new InvalidOperationException("Normalized value is required.");
        NormalizedValue = normalizedValue;
        UpdatedAtUtc = DateTime.UtcNow;
    }
}
