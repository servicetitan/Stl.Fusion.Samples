using System.Threading;
using System.Threading.Tasks;
using Samples.HelloBlazorHybrid.Abstractions;
using Stl.Fusion;

namespace Samples.HelloBlazorHybrid.Services;

public class CounterService : ICounterService
{
    private volatile int _count;

    [ComputeMethod]
    public virtual Task<int> Get(CancellationToken cancellationToken = default)
        => Task.FromResult(_count);

    public Task Increment(CancellationToken cancellationToken = default)
    {
        Interlocked.Increment(ref _count);
        using (Computed.Invalidate())
            Get(cancellationToken);
        return Task.CompletedTask;
    }
}
