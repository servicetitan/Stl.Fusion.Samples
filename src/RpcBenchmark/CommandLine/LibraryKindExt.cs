namespace Samples.RpcBenchmark;

public static class LibraryKindExt
{
    public static LibraryKind Parse(string value)
    {
        value = value.Trim().Replace("-", "").Replace(".", "").ToLowerInvariant();
        if (Enum.TryParse<LibraryKind>(value, true, out var result))
            return result;

        return value switch {
            "rpc" => LibraryKind.StlRpc,
            "sr" => LibraryKind.SignalR,
            "jsonrpc" => LibraryKind.StreamJsonRpc,
            "vsjsonrpc" => LibraryKind.StreamJsonRpc,
            "mo" => LibraryKind.MagicOnion,
            _ => throw new ArgumentOutOfRangeException(nameof(value)),
        };
    }
}
