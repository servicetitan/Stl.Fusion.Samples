using System;
using System.Threading;
using System.Threading.Tasks;
using Stl.Fusion;

namespace Samples.Blazor.Common.Services
{
    public interface ITimeService
    {
        [ComputeMethod(KeepAliveTime = 1)]
        Task<DateTime> GetTimeAsync(CancellationToken cancellationToken = default);
        [ComputeMethod(KeepAliveTime = 60)]
        Task<TimeSpan> GetUptimeAsync(TimeSpan updatePeriod, CancellationToken cancellationToken = default);
    }
}
