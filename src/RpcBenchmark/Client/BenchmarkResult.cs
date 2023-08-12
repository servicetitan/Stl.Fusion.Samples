namespace Samples.RpcBenchmark.Client;

public readonly record struct BenchmarkResult(long Count, double Duration)
{
    public static BenchmarkResult operator +(BenchmarkResult a, BenchmarkResult b)
        => new(a.Count + b.Count, a.Duration + b.Duration);
}
