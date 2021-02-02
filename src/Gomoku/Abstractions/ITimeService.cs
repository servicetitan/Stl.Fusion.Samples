using System;
using System.Threading;
using System.Threading.Tasks;
using Stl.Fusion;

namespace Samples.Gomoku.Abstractions
{
    public interface ITimeService
    {
        [ComputeMethod(KeepAliveTime = 1)]
        Task<DateTime> GetTimeAsync(CancellationToken cancellationToken = default);
    }
}
