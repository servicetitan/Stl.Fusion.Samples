using System;
using System.Threading;
using System.Threading.Tasks;
using Stl.Fusion;

namespace Samples.Blazor.Abstractions
{
    public interface ITimeService
    {
        [ComputeMethod(KeepAliveTime = 1)]
        Task<DateTime> GetTime(CancellationToken cancellationToken = default);
        [ComputeMethod(KeepAliveTime = 60)]
        Task<TimeSpan> GetUptime(TimeSpan updatePeriod, CancellationToken cancellationToken = default);
    }
}
