using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Stl.Fusion.Server;
using Samples.Blazor.Common.Services;

namespace Samples.Blazor.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController, JsonifyErrors]
    public class ScreenshotController : ControllerBase, IScreenshotService
    {
        private readonly IScreenshotService _screenshots;

        public ScreenshotController(IScreenshotService screenshots) => _screenshots = screenshots;

        [HttpGet("get"), Publish]
        public Task<Screenshot> GetScreenshotAsync(int width, CancellationToken cancellationToken)
            => _screenshots.GetScreenshotAsync(width, cancellationToken);
    }
}
