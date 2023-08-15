namespace Samples.RpcBenchmark;

public static class BenchmarkKindExt
{
    public static BenchmarkKind Parse(string value)
    {
        value = value.Trim().Replace("-", "").Replace(".", "").ToLowerInvariant();
        if (Enum.TryParse<BenchmarkKind>(value, true, out var result))
            return result;

        return value switch {
            "rpc" => BenchmarkKind.StlRpc,
            "jsonrpc" => BenchmarkKind.StreamJsonRpc,
            "vsjsonrpc" => BenchmarkKind.StreamJsonRpc,
            "sr" => BenchmarkKind.SignalR,
            _ => throw new ArgumentOutOfRangeException(nameof(value)),
        };
    }
}
