using System;
using System.Threading;
using System.Threading.Tasks;
using Stl.Fusion;
using Samples.Blazor.Abstractions;

namespace Samples.Blazor.Server.Services
{
    public class TimeService : ITimeService
    {
        private readonly DateTime _startTime = DateTime.UtcNow;

        [ComputeMethod(AutoInvalidateTime = 0.25, KeepAliveTime = 1)]
        public virtual async Task<DateTime> GetTime(CancellationToken cancellationToken = default)
        {
            var time = DateTime.Now;
            if (time.Second % 10 == 0)
                // This delay is here solely to let you see ServerTime page in
                // in "Loading" / "Updating" state.
                await Task.Delay(TimeSpan.FromSeconds(1), cancellationToken);
            return time;
        }

        public virtual Task<TimeSpan> GetUptime(TimeSpan updatePeriod, CancellationToken cancellationToken = default)
        {
            var computed = Computed.GetCurrent();
            Task.Delay(updatePeriod, default)
                .ContinueWith(_ => computed!.Invalidate(), CancellationToken.None);
            return Task.FromResult(DateTime.UtcNow - _startTime);
        }
    }
}
