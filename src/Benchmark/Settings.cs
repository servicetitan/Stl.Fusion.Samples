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
    public static readonly int WorkerCount = HardwareInfo.ProcessorCount * 30;
    public static readonly int? WriterFrequency = null;
    public static readonly int TestServiceConcurrency = 100;
    public static readonly TimeSpan Duration = TimeSpan.FromSeconds(5);
    public static readonly TimeSpan WarmupDuration = TimeSpan.FromSeconds(1);
    public static readonly bool ForceGCCollect = true;
    public const long TimeCheckCountMask = 3;
}
