using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Stl.Fusion;
using Samples.Blazor.Common.Services;

namespace Samples.Blazor.Server.Services
{
    [ComputedService(typeof(ITimeService))]
    public class TimeService : ITimeService, IComputedService
    {
        private readonly ILogger _log;

        public TimeService(ILogger<TimeService>? log = null)
            => _log = log ??= NullLogger<TimeService>.Instance;

        [ComputedServiceMethod(AutoInvalidateTime = 0.1)]
        public virtual Task<DateTime> GetTimeAsync(CancellationToken cancellationToken = default)
            => Task.FromResult(DateTime.Now);
    }
}
