namespace ControlEasyReborn.Modules.AccessControl.Domain.ValueObjects;

public enum ResultCountBand
{
    Zero = 0,
    One = 1,
    TwoToTen = 2,
    OverTen = 3
}

public static class ResultCountBandRules
{
    public const int OverTenThreshold = 10;

    public static ResultCountBand FromCount(int count) => count switch
    {
        0 => ResultCountBand.Zero,
        1 => ResultCountBand.One,
        <= OverTenThreshold => ResultCountBand.TwoToTen,
        _ => ResultCountBand.OverTen
    };

    public static string ToWire(ResultCountBand band) => band switch
    {
        ResultCountBand.Zero => "0",
        ResultCountBand.One => "1",
        ResultCountBand.TwoToTen => "2_to_10",
        ResultCountBand.OverTen => "over_10",
        _ => "unknown"
    };

    public static bool TryParse(string? value, out ResultCountBand band)
    {
        switch (value)
        {
            case "0": band = ResultCountBand.Zero; return true;
            case "1": band = ResultCountBand.One; return true;
            case "2_to_10": band = ResultCountBand.TwoToTen; return true;
            case "over_10": band = ResultCountBand.OverTen; return true;
            default: band = ResultCountBand.Zero; return false;
        }
    }
}
