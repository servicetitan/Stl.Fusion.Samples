using Stl.OS;

namespace Samples.Benchmark;

public static class Settings
{
    public static readonly string BaseUrl = "http://localhost:22333/";
    public static readonly string DbConnectionString =
        "Server=localhost;Database=stl_fusion_benchmark;Port=5432;User Id=postgres;Password=postgres";

    public static readonly int ItemCount = 1_000;
    public static readonly int WorkerCount = HardwareInfo.ProcessorCount * 10;
    public static readonly int? WriterFrequency = null;
    public static readonly int TestServiceConcurrency = 48;
    public static readonly TimeSpan Duration = TimeSpan.FromSeconds(10);
    public static readonly TimeSpan WarmupDuration = TimeSpan.FromSeconds(2);
    public static readonly bool ForceGCCollect = true;
    public const long TimeCheckCountMask = 7;
}
