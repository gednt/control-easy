namespace ControlEasyReborn.SharedKernel.Demo;

public static class DemoPersonas
{
    public static readonly IReadOnlySet<string> Emails = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
    {
        "platform@controleasy.app",
        "admin@controleasy.app",
        "porteiro@controleasy.app",
        "morador@controleasy.app",
        "multi@controleasy.app",
    };

    public static bool IsDemoPersona(string? email) =>
        !string.IsNullOrWhiteSpace(email) && Emails.Contains(email.Trim());
}
