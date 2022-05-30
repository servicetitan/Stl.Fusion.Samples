using Microsoft.AspNetCore.Mvc;
using Samples.HelloBlazorHybrid.Abstractions;
using Stl.Fusion.Server;

namespace Samples.HelloBlazorHybrid.Server.Controllers;

[Route("api/[controller]/[action]")]
[ApiController, JsonifyErrors, UseDefaultSession]
public class CounterController : ControllerBase, ICounterService
{
    private readonly ICounterService _counter;

    public CounterController(ICounterService counter) 
        => _counter = counter;

    [HttpGet, Publish]
    public Task<int> Get(CancellationToken cancellationToken = default)
        => _counter.Get(cancellationToken);

    [HttpPost]
    public Task Increment(CancellationToken cancellationToken = default)
        => _counter.Increment(cancellationToken);
}
