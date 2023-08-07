using Stl.OS;

namespace Samples.RpcBenchmark;

public static class Settings
{
    public static readonly string BaseUrl = "http://localhost:22444/";

    public static readonly int TestServiceConcurrency = 100;
    public static readonly int WorkerCount = HardwareInfo.ProcessorCount * 100;
    public static readonly TimeSpan Duration = TimeSpan.FromSeconds(5);
    public static readonly TimeSpan WarmupDuration = TimeSpan.FromSeconds(1);
    public static readonly bool ForceGCCollect = true;
    public const long TimeCheckCountMask = 15;
}
