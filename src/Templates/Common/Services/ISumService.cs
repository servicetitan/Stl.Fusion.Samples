using System.Threading;
using System.Threading.Tasks;
using Stl.Fusion;

namespace Templates.Blazor.Common.Services
{
    public interface ISumService
    {
        Task ResetAsync(CancellationToken cancellationToken = default);
        Task AccumulateAsync(double value, CancellationToken cancellationToken = default);

        [ComputeMethod]
        Task<double> GetAccumulatorAsync(CancellationToken cancellationToken = default);
        [ComputeMethod]
        Task<double> SumAsync(double[] values, bool addAccumulator, CancellationToken cancellationToken = default);
    }
}
