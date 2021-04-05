using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Stl.Fusion.Server;
using Samples.Blazor.Abstractions;
using Stl.Fusion.Authentication;

namespace Samples.Blazor.Server.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController, JsonifyErrors]
    public class ComposerController : ControllerBase, IComposerService
    {
        private readonly IComposerService _composer;

        public ComposerController(IComposerService composer) => _composer = composer;

        [HttpGet, Publish]
        public Task<ComposedValue> GetComposedValue(string? parameter, Session session, CancellationToken cancellationToken = default)
        {
            parameter ??= "";
            return _composer.GetComposedValue(parameter, session, cancellationToken);
        }
    }
}
