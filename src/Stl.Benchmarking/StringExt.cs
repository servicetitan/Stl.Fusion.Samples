namespace Stl.Benchmarking;

public static class StringExt
{
    public static string NormalizeBaseUrl(this string url)
        => url.EnsureHasSuffix("/");

    public static string EnsureHasSuffix(this string value, string suffix)
        => value.EndsWith(suffix, StringComparison.Ordinal)
            ? value
            : value + suffix;
}
