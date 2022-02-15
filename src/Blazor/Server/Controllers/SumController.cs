using Microsoft.AspNetCore.Mvc;
using Samples.Blazor.Abstractions;
using Stl.Fusion.Server;

namespace Samples.Blazor.Server.Controllers;

[Route("api/[controller]/[action]")]
[ApiController, JsonifyErrors]
public class SumController : ControllerBase, ISumService
{
    private readonly ISumService _sumService;

    public SumController(ISumService sumService) => _sumService = sumService;

    [HttpPost]
    public Task Reset(CancellationToken cancellationToken)
        => _sumService.Reset(cancellationToken);

    [HttpPost]
    public Task Accumulate(double value, CancellationToken cancellationToken)
        => _sumService.Accumulate(value, cancellationToken);

    [HttpGet, Publish]
    public Task<double> GetAccumulator(CancellationToken cancellationToken)
        => _sumService.GetAccumulator(cancellationToken);

    [HttpGet, Publish]
    public Task<double> GetSum([FromQuery] double[] values, bool addAccumulator, CancellationToken cancellationToken)
        => _sumService.GetSum(values, addAccumulator, cancellationToken);
}
