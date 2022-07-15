namespace Samples.Blazor.Abstractions;

public interface ITimeService
{
    [ComputeMethod]
    Task<DateTime> GetTime(CancellationToken cancellationToken = default);
    [ComputeMethod]
    Task<double> GetUptime(double updatePeriod, CancellationToken cancellationToken = default);
}
