namespace Samples.RpcBenchmark;

public static class SystemSettings
{
    private static bool _isApplied;

    public static void Apply(BenchmarkCommandBase command)
    {
        if (_isApplied)
            return;

        // Thread pool
        ThreadPool.GetMinThreads(out var minWorkerThreads, out var minIOThreads);
        minWorkerThreads = Math.Max(minWorkerThreads, command.MinWorkerThreads);
        minIOThreads = Math.Max(minIOThreads, command.MinIOThreads);
        ThreadPool.SetMinThreads(minWorkerThreads, minIOThreads);
        ThreadPool.SetMaxThreads(16_384, 16_384);

        // Stl.Rpc serializer
        var serializer = command.Serializer switch {
            SerializerKind.MemoryPack => (IByteSerializer)MemoryPackByteSerializer.Default,
            SerializerKind.MessagePack => MessagePackByteSerializer.Default,
            _ => throw new ArgumentOutOfRangeException(nameof(command.Serializer))
        };
        ByteSerializer.Default = serializer;

        WriteLine("System-wide settings:");
        WriteLine($"  Thread pool settings: {minWorkerThreads}+ worker, {minIOThreads}+ I/O threads");
        WriteLine($"  Stl.Rpc serializer:   {command.Serializer}");
        _isApplied = true;
    }

}
