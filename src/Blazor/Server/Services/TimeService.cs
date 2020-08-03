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

        [ComputeMethod(AutoInvalidateTime = 0.1)]
        public virtual Task<DateTime> GetTimeAsync(CancellationToken cancellationToken = default)
            => Task.FromResult(DateTime.Now);
    }
}
