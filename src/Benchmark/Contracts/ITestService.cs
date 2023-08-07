using Stl.Rpc;

namespace Samples.Benchmark;

public interface ITestService
{
    Task AddOrUpdate(TestItem item, long? version, CancellationToken cancellationToken = default);
    Task Remove(long itemId, long version, CancellationToken cancellationToken = default);

    // Compute methods
    [ComputeMethod(MinCacheDuration = 10)]
    Task<TestItem[]> GetAll(CancellationToken cancellationToken = default);
    [ComputeMethod(MinCacheDuration = 10)]
    Task<TestItem?> TryGet(long itemId, CancellationToken cancellationToken = default);
}

public interface IRpcTestService : ITestService, IRpcService
{ }

public interface IFusionTestService : IRpcTestService, IComputeService
{ }
