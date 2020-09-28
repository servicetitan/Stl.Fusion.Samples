using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Stl.Fusion;
using Samples.Blazor.Common.Services;

namespace Samples.Blazor.Server.Services
{
    [ComputeService(typeof(ITimeService))]
    public class TimeService : ITimeService
    {
        private readonly ILogger _log;

        public TimeService(ILogger<TimeService>? log = null)
            => _log = log ??= NullLogger<TimeService>.Instance;

        [ComputeMethod(AutoInvalidateTime = 0.25)]
        public virtual async Task<DateTime> GetTimeAsync(CancellationToken cancellationToken = default)
        {
            var time = DateTime.Now;
            if (time.Second % 10 == 0)
                // This delay is here solely to let you see ServerTime page in
                // in "Loading" / "Updating" state.
                await Task.Delay(TimeSpan.FromSeconds(1), cancellationToken).ConfigureAwait(false);
            return time;
        }
    }
}
