using System.Diagnostics;
using Samples.HelloBlazorHybrid.Abstractions;
using Stl.Fusion.Extensions;

namespace Samples.HelloBlazorHybrid.Services;

// This is an optional service allowing to report backend connectivity status.
// You normally don't need to override the default one, but here
// it's done only because there is no IAuth service registered in this sample.
public class CustomBackendStatus : BackendStatus
{
    private readonly ICounterService _counterService;
    private readonly ILogger _log;

    public CustomBackendStatus(ICounterService counterService, ILogger<CustomBackendStatus> log)
        : base(null!)
    {
        _log = log;
        _counterService = counterService;
    }

    [ComputeMethod]
    protected override async Task<Unit> HitBackend(
        Session session,
        string backend,
        CancellationToken cancellationToken = default)
    {
        await _counterService.Get(cancellationToken);
        return default;
    }
}
