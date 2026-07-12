namespace ControlEasyReborn.SharedKernel.Demo;

public sealed class DemoOptions
{
    public const string SectionName = "Demo";

    public bool Enabled { get; init; }
    public int SeedVersion { get; init; } = 1;
    public bool DisableOutboundEmail { get; init; }
}
