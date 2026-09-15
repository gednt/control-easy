namespace ControlEasyReborn.Modules.AccessControl.Domain.ValueObjects;

public enum CredentialMethod
{
    Qr = 0,
    FacialBiometricReserved = 99
}

public static class CredentialMethodCodes
{
    public static readonly IReadOnlyDictionary<string, CredentialMethod> ParseTable = new Dictionary<string, CredentialMethod>(StringComparer.OrdinalIgnoreCase)
    {
        ["qr"] = CredentialMethod.Qr,
        ["facial_biometric"] = CredentialMethod.FacialBiometricReserved
    };

    public static bool TryParse(string? value, out CredentialMethod method)
    {
        if (!string.IsNullOrWhiteSpace(value) && ParseTable.TryGetValue(value, out method))
        {
            return true;
        }
        method = CredentialMethod.Qr;
        return false;
    }

    public static string ToWire(CredentialMethod method) => method switch
    {
        CredentialMethod.Qr => "qr",
        CredentialMethod.FacialBiometricReserved => "facial_biometric",
        _ => "unknown"
    };

    public static bool IsCreatable(CredentialMethod method) => method == CredentialMethod.Qr;
}
