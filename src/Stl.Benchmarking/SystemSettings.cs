using MessagePack;

namespace Stl.Benchmarking;

public static class SystemSettings
{
    private static readonly object Lock = new();
    private static bool _isApplied;

    public static void Apply(int minWorkerThreads, int minIOThreads, ByteSerializerKind serializerKind)
    {
        lock (Lock) {
            if (_isApplied)
                return;

            // Thread pool
            ThreadPool.GetMinThreads(out var currentMinWorkerThreads, out var currentMinIOThreads);
            currentMinWorkerThreads = Math.Max(currentMinWorkerThreads, minWorkerThreads);
            currentMinIOThreads = Math.Max(currentMinIOThreads, minIOThreads);
            ThreadPool.SetMinThreads(currentMinWorkerThreads, currentMinIOThreads);
            ThreadPool.SetMaxThreads(16_384, 16_384);

            // Stl.Rpc serializer
            var serializer = serializerKind switch {
                ByteSerializerKind.MemoryPack => (IByteSerializer)MemoryPackByteSerializer.Default,
                ByteSerializerKind.MessagePack => MessagePackByteSerializer.Default,
                _ => throw new ArgumentOutOfRangeException(nameof(serializerKind))
            };
            ByteSerializer.Default = serializer;

            WriteLine("System-wide settings:");
            WriteLine($"  Thread pool settings:   {currentMinWorkerThreads}+ worker, {currentMinIOThreads}+ I/O threads");
            WriteLine($"  ByteSerializer.Default: {serializerKind}");
            _isApplied = true;
        }
    }

}
