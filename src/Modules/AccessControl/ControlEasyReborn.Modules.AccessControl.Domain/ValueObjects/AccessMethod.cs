namespace ControlEasyReborn.Modules.AccessControl.Domain.ValueObjects;

public enum AccessMethod
{
    Qr = 0,
    ManualLookup = 1
}

public static class AccessMethodCodes
{
    public static readonly IReadOnlyDictionary<string, AccessMethod> ParseTable = new Dictionary<string, AccessMethod>(StringComparer.OrdinalIgnoreCase)
    {
        ["qr"] = AccessMethod.Qr,
        ["manual_lookup"] = AccessMethod.ManualLookup
    };

    public static bool TryParse(string? value, out AccessMethod method)
    {
        if (!string.IsNullOrWhiteSpace(value) && ParseTable.TryGetValue(value, out method))
        {
            return true;
        }
        method = AccessMethod.Qr;
        return false;
    }

    public static string ToWire(AccessMethod method) => method switch
    {
        AccessMethod.Qr => "qr",
        AccessMethod.ManualLookup => "manual_lookup",
        _ => "unknown"
    };
}
