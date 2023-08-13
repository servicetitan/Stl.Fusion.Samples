namespace Stl.Benchmarking;

public static class FormatExt
{
    public static string FormatCount(this double value)
    {
        var scale = "";
        if (value >= 1000_000) {
            scale = "M";
            value /= 1000_000;
        }
        else if (value >= 1000) {
            scale = "K";
            value /= 1000;
        }
        return $"{value:N}{scale}";
    }

    public static string FormatDuration(this double value)
    {
        var scale = "";
        if (value >= 1d)
            scale = "s";
        else if (value >= 0.001) {
            scale = "ms";
            value *= 1000;
        }
        else if (value >= 0.000_001) {
            scale = "us";
            value *= 1000_000;
        }
        else if (value >= 0.000_000_001) {
            scale = "ns";
            value *= 1000_000_000;
        }
        return $"{value:N}{scale}";
    }
}
