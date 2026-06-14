namespace ControlEasyReborn.SharedKernel.Bootstrap;

public sealed class BootstrapOptions
{
    public const string SectionName = "Bootstrap";

    public string? PlatformAdminEmail { get; init; }
    public string? PlatformAdminPassword { get; init; }
}
