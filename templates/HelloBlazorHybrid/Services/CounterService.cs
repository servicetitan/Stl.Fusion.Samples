using System.Threading;
using System.Threading.Tasks;
using HelloBlazorHybrid.Abstractions;
using Stl.Fusion;

namespace HelloBlazorHybrid.Services
{
    public class CounterService : ICounterService
    {
        private readonly object _lock = new();
        private int _count;

        [ComputeMethod]
        public virtual Task<int> Get(CancellationToken cancellationToken = default)
        {
            lock (_lock) {
                return Task.FromResult(_count);
            }
        }
        
        public Task Increment(CancellationToken cancellationToken = default)
        {
            lock (_lock) {
                ++_count;
            }
            using (Computed.Invalidate())
                Get(cancellationToken);
            return Task.CompletedTask;
        }
    }
}
