using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Samples.Blazor.Common.Services;
using Stl.Fusion.Bridge;
using Stl.Fusion.Server;

namespace Samples.Blazor.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SumController : FusionController, ISumService
    {
        private readonly ISumService _sumService;

        public SumController(ISumService sumService, IPublisher publisher)
            : base(publisher) =>
            _sumService = sumService;

        [HttpPost("reset")]
        public Task ResetAsync(CancellationToken cancellationToken)
            => _sumService.ResetAsync(cancellationToken);

        [HttpPost("accumulate")]
        public Task AccumulateAsync(double value, CancellationToken cancellationToken)
            => _sumService.AccumulateAsync(value, cancellationToken);

        [HttpGet("getAccumulator")]
        public Task<double> GetAccumulatorAsync(CancellationToken cancellationToken)
            => PublishAsync(ct => _sumService.GetAccumulatorAsync(ct), cancellationToken);

        [HttpGet("sum")]
        public Task<double> SumAsync([FromQuery] double[] values, bool addAccumulator, CancellationToken cancellationToken)
            => PublishAsync(ct => _sumService.SumAsync(values, addAccumulator, ct), cancellationToken);
    }
}
