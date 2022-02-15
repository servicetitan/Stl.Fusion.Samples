using Microsoft.AspNetCore.Mvc;
using Stl.Fusion.Server;
using Samples.Blazor.Abstractions;

namespace Samples.Blazor.Server.Controllers;

[Route("api/[controller]/[action]")]
[ApiController, JsonifyErrors]
public class ScreenshotController : ControllerBase, IScreenshotService
{
    private readonly IScreenshotService _screenshots;

    public ScreenshotController(IScreenshotService screenshots) => _screenshots = screenshots;

    [HttpGet, Publish]
    public Task<Screenshot> GetScreenshot(int width, CancellationToken cancellationToken)
        => _screenshots.GetScreenshot(width, cancellationToken);
}
