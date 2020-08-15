using System;
using System.Threading;
using System.Threading.Tasks;
using Stl.Fusion;

namespace Samples.Blazor.Common.Services
{
    public interface ITimeService
    {
        [ComputeMethod]
        Task<DateTime> GetTimeAsync(CancellationToken cancellationToken = default);
    }
}
