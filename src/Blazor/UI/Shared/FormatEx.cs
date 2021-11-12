namespace Samples.Blazor.UI.Shared;

public static class FormatEx
{
    public static string Format(this DateTime dateTime)
        => dateTime.ToString("HH:mm:ss.ffff");

    public static string Format(this DateTime? dateTime)
        => dateTime?.ToString("HH:mm:ss.ffff") ?? "n/a";
}
