namespace Samples.RpcBenchmark.Client;

public static class Int64Ext
{
    public static string FormatRps(this long count, TimeSpan duration)
    {
        var scale = "";
        var value = count / duration.TotalSeconds;
        if (value >= 1000_000) {
            scale = "M";
            value /= 1000_000;
        }
        else if (value >= 1000) {
            scale = "K";
            value /= 1000;
        }
        return $"{value:N}{scale} requests/s";
    }
}
