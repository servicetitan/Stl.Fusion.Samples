using System.Threading;
using System.Threading.Tasks;
using Stl.Fusion;

namespace Samples.Blazor.Abstractions;

public interface ISumService
{
    Task Reset(CancellationToken cancellationToken = default);
    Task Accumulate(double value, CancellationToken cancellationToken = default);

    [ComputeMethod]
    Task<double> GetAccumulator(CancellationToken cancellationToken = default);
    [ComputeMethod]
    Task<double> GetSum(double[] values, bool addAccumulator, CancellationToken cancellationToken = default);
}
