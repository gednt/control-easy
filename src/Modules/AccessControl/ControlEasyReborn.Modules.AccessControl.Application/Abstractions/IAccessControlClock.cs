namespace ControlEasyReborn.Modules.AccessControl.Application.Abstractions;

public interface IAccessControlClock
{
    DateTime UtcNow { get; }
}