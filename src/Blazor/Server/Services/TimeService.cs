using System;
using System.Threading;
using System.Threading.Tasks;
using Stl.Fusion;
using Samples.Blazor.Common.Services;

namespace Samples.Blazor.Server.Services
{
    [ComputeService(typeof(ITimeService))]
    public class TimeService : ITimeService
    {
        private DateTime _startTime = DateTime.UtcNow;

        [ComputeMethod(AutoInvalidateTime = 0.25, KeepAliveTime = 1)]
        public virtual async Task<DateTime> GetTimeAsync(CancellationToken cancellationToken = default)
        {
            var time = DateTime.Now;
            if (time.Second % 10 == 0)
                // This delay is here solely to let you see ServerTime page in
                // in "Loading" / "Updating" state.
                await Task.Delay(TimeSpan.FromSeconds(1), cancellationToken).ConfigureAwait(false);
            return time;
        }

        public virtual Task<TimeSpan> GetUptimeAsync(TimeSpan updatePeriod, CancellationToken cancellationToken = default)
        {
            var computed = Computed.GetCurrent();
            Task.Delay(updatePeriod, cancellationToken).ContinueWith(_ => computed!.Invalidate(), cancellationToken);
            return Task.FromResult(DateTime.UtcNow - _startTime);
        }
    }
}
