namespace Samples.HelloBlazorHybrid.Abstractions;

public interface ICounterService : IComputeService
{
    [ComputeMethod]
    Task<int> Get(CancellationToken cancellationToken = default);
    Task Increment(CancellationToken cancellationToken = default);
}
