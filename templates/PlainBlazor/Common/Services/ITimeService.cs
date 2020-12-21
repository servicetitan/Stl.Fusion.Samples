using System;
using System.Threading;
using System.Threading.Tasks;
using Stl.Fusion;

namespace Templates.Blazor.Common.Services
{
    public interface ITimeService
    {
        [ComputeMethod(KeepAliveTime = 1)]
        Task<DateTime> GetTimeAsync(CancellationToken cancellationToken = default);
    }
}
