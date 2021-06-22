using System.Threading;
using System.Threading.Tasks;
using Stl.Fusion;

namespace HelloBlazorHybrid.Abstractions
{
    public interface ICounterService
    {
        [ComputeMethod]
        Task<int> Get(CancellationToken cancellationToken = default);
        Task Increment(CancellationToken cancellationToken = default);
    }
}