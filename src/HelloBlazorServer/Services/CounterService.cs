using System.Threading;
using System.Threading.Tasks;
using Stl.Fusion;

namespace Samples.HelloBlazorServer.Services
{
    [ComputeService]
    public class CounterService
    {
        private int _value;

        [ComputeMethod]
        public virtual Task<int> GetCounterAsync(CancellationToken cancellationToken = default)
            => Task.FromResult(_value);

        public Task IncrementCounterAsync(CancellationToken cancellationToken = default)
        {
            ++_value;
            Computed.Invalidate(() => GetCounterAsync(default));
            return Task.CompletedTask;
        }
    }
}
