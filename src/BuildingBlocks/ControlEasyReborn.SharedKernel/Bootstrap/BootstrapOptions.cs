namespace ControlEasyReborn.SharedKernel.Bootstrap;

public sealed class BootstrapOptions
{
    public const string SectionName = "Bootstrap";

    public bool Enabled { get; init; } = true;

    public string? PlatformAdminEmail { get; init; }
    public string? PlatformAdminPassword { get; init; }
}
