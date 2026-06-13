using ControlEasyReborn.SharedKernel.Demo;
using Microsoft.Extensions.Configuration;

namespace ControlEasyReborn.Infrastructure.Demo;

public static class DemoHostingExtensions
{
    public static bool ShouldRegisterPlatformAdminBootstrap(IConfiguration configuration) =>
        !configuration.GetValue<bool>($"{DemoOptions.SectionName}:Enabled");
}
