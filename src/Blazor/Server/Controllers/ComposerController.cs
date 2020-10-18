using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Stl.Fusion.Bridge;
using Stl.Fusion.Server;
using Samples.Blazor.Common.Services;
using Stl.Fusion.Authentication;

namespace Samples.Blazor.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ComposerController : FusionController, IComposerService
    {
        private readonly IComposerService _composer;

        public ComposerController(IComposerService composer) => _composer = composer;

        [HttpGet("get"), Publish]
        public Task<ComposedValue> GetComposedValueAsync(string? parameter, Session session, CancellationToken cancellationToken = default)
        {
            parameter ??= "";
            return _composer.GetComposedValueAsync(parameter, session, cancellationToken);
        }
    }
}
