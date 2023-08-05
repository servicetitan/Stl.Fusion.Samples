namespace Samples.Blazor.Abstractions;

public interface ITimeService : IComputeService
{
    [ComputeMethod]
    Task<DateTime> GetTime(CancellationToken cancellationToken = default);
    [ComputeMethod]
    Task<double> GetUptime(double updatePeriod, CancellationToken cancellationToken = default);
}
