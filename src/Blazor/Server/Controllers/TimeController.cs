using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Stl.Fusion.Bridge;
using Stl.Fusion.Server;
using Samples.Blazor.Common.Services;

namespace Samples.Blazor.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TimeController : FusionController, ITimeService
    {
        private readonly ITimeService _time;

        public TimeController(ITimeService time) => _time = time;

        [HttpGet("get"), Publish]
        public Task<DateTime> GetTimeAsync(CancellationToken cancellationToken)
            => _time.GetTimeAsync(cancellationToken);

        [HttpGet("getUptime"), Publish]
        public Task<TimeSpan> GetUptimeAsync(TimeSpan updatePeriod, CancellationToken cancellationToken = default)
            => _time.GetUptimeAsync(updatePeriod, cancellationToken);
    }
}
