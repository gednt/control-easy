namespace ControlEasyReborn.Modules.AccessControl.Domain.ValueObjects;

public enum CycleDirection
{
    Entrance = 0,
    Exit = 1
}

public static class CycleDirectionCodes
{
    public static readonly IReadOnlyDictionary<string, CycleDirection> ParseTable = new Dictionary<string, CycleDirection>(StringComparer.OrdinalIgnoreCase)
    {
        ["entrance"] = CycleDirection.Entrance,
        ["exit"] = CycleDirection.Exit
    };

    public static bool TryParse(string? value, out CycleDirection direction)
    {
        if (!string.IsNullOrWhiteSpace(value) && ParseTable.TryGetValue(value, out direction))
        {
            return true;
        }
        direction = CycleDirection.Entrance;
        return false;
    }

    public static string ToWire(CycleDirection direction) => direction switch
    {
        CycleDirection.Entrance => "entrance",
        CycleDirection.Exit => "exit",
        _ => "unknown"
    };
}
