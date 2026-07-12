namespace ControlEasyReborn.Modules.Vehicles.Domain.ValueObjects;

public sealed record LicensePlate
{
    public string Value { get; }

    public LicensePlate(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("License plate cannot be empty.", nameof(value));

        if (value.Length > 10)
            throw new ArgumentException("License plate cannot exceed 10 characters.", nameof(value));

        Value = value.ToUpperInvariant();
    }

    public override string ToString() => Value;
}