using ControlEasyReborn.Modules.AccessControl.Application.Abstractions;

namespace ControlEasyReborn.Modules.AccessControl.Infrastructure.Services;

public sealed class SystemAccessControlClock : IAccessControlClock
{
    public DateTime UtcNow => DateTime.UtcNow;
}