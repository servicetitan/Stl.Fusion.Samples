using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Stl.Fusion.Server;
using Samples.BoardGames.Abstractions;

namespace Samples.BoardGames.Host.Controllers
{
    [Route("api/[controller]")]
    [ApiController, JsonifyErrors]
    public class TimeController : ControllerBase, ITimeService
    {
        private readonly ITimeService _time;

        public TimeController(ITimeService time) => _time = time;

        [HttpGet("get"), Publish]
        public Task<DateTime> GetTimeAsync(CancellationToken cancellationToken)
            => _time.GetTimeAsync(cancellationToken);
    }
}
