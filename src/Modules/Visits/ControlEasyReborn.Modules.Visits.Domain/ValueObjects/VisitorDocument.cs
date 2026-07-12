namespace ControlEasyReborn.Modules.Visits.Domain.ValueObjects;

public sealed record VisitorDocument
{
    public string Value { get; }

    public VisitorDocument(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("Visitor document cannot be empty.", nameof(value));

        if (value.Length > 20)
            throw new ArgumentException("Visitor document cannot exceed 20 characters.", nameof(value));

        Value = value;
    }
}