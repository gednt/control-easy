namespace ControlEasyReborn.Modules.Administration.Domain.Entities;

public sealed class ConfigurationEntry
{
    public Guid Id { get; private set; }
    public Guid TenantId { get; private set; }
    public string Key { get; private set; } = string.Empty;
    public string Value { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    public DateTime CreatedAtUtc { get; private set; }
    public DateTime? UpdatedAtUtc { get; private set; }

    private ConfigurationEntry() { }

    public ConfigurationEntry(Guid id, Guid tenantId, string key, string value, string? description, DateTime createdAtUtc, DateTime? updatedAtUtc = null)
    {
        Id = id;
        TenantId = tenantId;
        Key = key;
        Value = value;
        Description = description;
        CreatedAtUtc = createdAtUtc;
        UpdatedAtUtc = updatedAtUtc;
    }

    public void UpdateValue(string value)
    {
        Value = value;
        UpdatedAtUtc = DateTime.UtcNow;
    }
}