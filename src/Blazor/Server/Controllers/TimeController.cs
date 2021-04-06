using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Stl.Fusion.Server;
using Samples.Blazor.Abstractions;

namespace Samples.Blazor.Server.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController, JsonifyErrors]
    public class TimeController : ControllerBase, ITimeService
    {
        private readonly ITimeService _time;

        public TimeController(ITimeService time) => _time = time;

        [HttpGet, Publish]
        public Task<DateTime> GetTime(CancellationToken cancellationToken)
            => _time.GetTime(cancellationToken);

        [HttpGet, Publish]
        public Task<TimeSpan> GetUptime(TimeSpan updatePeriod, CancellationToken cancellationToken = default)
            => _time.GetUptime(updatePeriod, cancellationToken);
    }
}
