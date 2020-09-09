using System;
using System.Threading;
using System.Threading.Tasks;
using Stl.Fusion;

namespace Samples.HelloBlazorServer.Services
{
    [ComputeService]
    public class CounterService
    {
        private readonly object _lock = new object();
        private int _count;
        private DateTime _changeTime = DateTime.Now;

        [ComputeMethod]
        public virtual Task<(int, DateTime)> GetCounterAsync(CancellationToken cancellationToken = default)
        {
            lock (_lock) {
                return Task.FromResult((_count, _changeTime));
            }
        }

        public Task IncrementCounterAsync(CancellationToken cancellationToken = default)
        {
            lock (_lock) {
                ++_count;
                _changeTime = DateTime.Now;
            }
            Computed.Invalidate(() => GetCounterAsync(default));
            return Task.CompletedTask;
        }
    }
}
