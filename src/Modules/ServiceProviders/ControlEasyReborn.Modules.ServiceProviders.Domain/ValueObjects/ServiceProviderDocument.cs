namespace ControlEasyReborn.Modules.ServiceProviders.Domain.ValueObjects;

public sealed record ServiceProviderDocument
{
    public string Value { get; }

    public ServiceProviderDocument(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("Document cannot be empty.", nameof(value));

        if (value.Length > 20)
            throw new ArgumentException("Document cannot exceed 20 characters.", nameof(value));

        Value = value;
    }

    public override string ToString() => Value;
}