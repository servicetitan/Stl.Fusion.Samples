using Stl.OS;

namespace Samples.RpcBenchmark;

public static class Settings
{
    public static readonly int Port = 22444;
    public static readonly string BaseUrl = $"https://localhost:{Port}/";

    public static readonly int WorkerCount = HardwareInfo.ProcessorCount * 100;
    public static readonly int ClientConcurrency = 80;
    public static readonly int GrpcWorkerCount = WorkerCount;
    public static readonly int GrpcClientConcurrency = ClientConcurrency * 5;
    public static readonly TimeSpan Duration = TimeSpan.FromSeconds(10);
    public static readonly TimeSpan WarmupDuration = TimeSpan.FromSeconds(5);
    public static readonly bool ForceGCCollect = true;
    public const long TimeCheckCountMask = 7;
}
