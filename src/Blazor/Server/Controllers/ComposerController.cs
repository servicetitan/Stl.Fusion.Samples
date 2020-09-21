using System;
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
        private readonly IAuthSessionAccessor _authSessionAccessor;

        public ComposerController(
            IComposerService composer, IAuthSessionAccessor authSessionAccessor,
            IPublisher publisher)
            : base(publisher)
        {
            _composer = composer;
            _authSessionAccessor = authSessionAccessor;
        }

        [HttpGet("get")]
        public Task<ComposedValue> GetComposedValueAsync(string? parameter, AuthSession? session, CancellationToken cancellationToken = default)
        {
            parameter ??= "";
            session ??= _authSessionAccessor.Session;
            return PublishAsync(ct => _composer.GetComposedValueAsync(parameter, session!, ct));
        }
    }
}
