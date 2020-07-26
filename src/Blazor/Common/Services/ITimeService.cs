using System;
using System.Threading;
using System.Threading.Tasks;

namespace Samples.Blazor.Common.Services
{
    public interface ITimeService
    {
        Task<DateTime> GetTimeAsync(CancellationToken cancellationToken = default);
    }
}
