using Stl.OS;

namespace Samples.Benchmark;

public static class Settings
{
    public static readonly string BaseUrl = "http://localhost:22333/";
    public static readonly string DbConnectionString =
        "Server=localhost;Database=stl_fusion_benchmark;Port=5432;" +
        "User Id=postgres;Password=postgres;" +
        "Minimum Pool Size=20;Maximum Pool Size=200;Multiplexing=true";

    public static readonly int ItemCount = 1_000;
    public static readonly int ReaderCount = HardwareInfo.ProcessorCount * 10;
    public static readonly int WriterCount = 1;
    public static readonly int TestServiceConcurrency = 200;
    public static readonly double Duration = 5; // In seconds
    public static readonly double WarmupDuration = 1; // In seconds
    public static readonly int TryCount = 4;

    public static CancellationToken StopToken;
}
