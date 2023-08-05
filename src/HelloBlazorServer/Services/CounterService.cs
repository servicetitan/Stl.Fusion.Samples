namespace Samples.HelloBlazorServer.Services;

public class CounterService : IComputeService
{
    private readonly object _lock = new();
    private int _count;
    private DateTime _changeTime = DateTime.Now;

    [ComputeMethod]
    public virtual Task<(int, DateTime)> Get()
    {
        lock (_lock) {
            return Task.FromResult((_count, _changeTime));
        }
    }

    public Task Increment()
    {
        lock (_lock) {
            ++_count;
            _changeTime = DateTime.Now;
        }
        using (Computed.Invalidate())
            Get();
        return Task.CompletedTask;
    }
}
