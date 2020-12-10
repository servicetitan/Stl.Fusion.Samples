using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Templates.Blazor.Common.Services;
using Stl.Fusion;

namespace Templates.Blazor.Server.Services
{
    [ComputeService(typeof(ISumService))]
    public class SumService : ISumService
    {
        private readonly IMutableState<double> _accumulator;

        public SumService(IStateFactory stateFactory)
            => _accumulator = stateFactory.NewMutable<double>();

        public Task ResetAsync(CancellationToken cancellationToken)
        {
            _accumulator.Value = 0;
            return Task.CompletedTask;
        }

        public Task AccumulateAsync(double value, CancellationToken cancellationToken)
        {
            _accumulator.Value += value;
            return Task.CompletedTask;
        }

        // Compute methods

        public virtual async Task<double> GetAccumulatorAsync(CancellationToken cancellationToken)
            => await _accumulator.UseAsync(cancellationToken).ConfigureAwait(false);

        public virtual async Task<double> SumAsync(double[] values, bool addAccumulator, CancellationToken cancellationToken)
        {
            var sum = values.Sum();
            if (addAccumulator)
                sum += await GetAccumulatorAsync(cancellationToken).ConfigureAwait(false);
            return sum;
        }
    }
}
