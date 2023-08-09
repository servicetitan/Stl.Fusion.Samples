using Samples.Blazor.Abstractions;

namespace Samples.Blazor.Server.Services;

public class TimeService : ITimeService
{
    private readonly DateTime _startTime = DateTime.UtcNow;

    [ComputeMethod(AutoInvalidationDelay = 0.25)]
    public virtual async Task<DateTime> GetTime(CancellationToken cancellationToken = default)
    {
        var time = DateTime.Now;
        if (time.Second % 10 == 0)
            // This delay is here solely to let you see ServerTime page in
            // in "Loading" / "Updating" state.
            await Task.Delay(TimeSpan.FromSeconds(1), cancellationToken);
        return time;
    }

    public virtual Task<double> GetUptime(double updatePeriod, CancellationToken cancellationToken = default)
    {
        var computed = Computed.GetCurrent();
        Task.Delay(TimeSpan.FromSeconds(updatePeriod), CancellationToken.None)
            .ContinueWith(_ => computed!.Invalidate(), CancellationToken.None);
        return Task.FromResult((DateTime.UtcNow - _startTime).TotalSeconds);
    }
}
