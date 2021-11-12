using Samples.Blazor.Abstractions;

namespace Samples.Blazor.Server.Services;

public class SumService : ISumService
{
    private readonly IMutableState<double> _accumulator;

    public SumService(IStateFactory stateFactory)
        => _accumulator = stateFactory.NewMutable<double>();

    public Task Reset(CancellationToken cancellationToken)
    {
        _accumulator.Value = 0;
        return Task.CompletedTask;
    }

    public Task Accumulate(double value, CancellationToken cancellationToken)
    {
        _accumulator.Value += value;
        return Task.CompletedTask;
    }

    // Compute methods

    public virtual async Task<double> GetAccumulator(CancellationToken cancellationToken)
        => await _accumulator.Use(cancellationToken);

    public virtual async Task<double> GetSum(double[] values, bool addAccumulator, CancellationToken cancellationToken)
    {
        var sum = values.Sum();
        if (addAccumulator)
            sum += await GetAccumulator(cancellationToken);
        return sum;
    }
}
