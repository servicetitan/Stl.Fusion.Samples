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

        public SumController(ISumService sumService) => _sumService = sumService;

        [HttpPost("reset")]
        public Task ResetAsync(CancellationToken cancellationToken)
            => _sumService.ResetAsync(cancellationToken);

        [HttpPost("accumulate")]
        public Task AccumulateAsync(double value, CancellationToken cancellationToken)
            => _sumService.AccumulateAsync(value, cancellationToken);

        [HttpGet("getAccumulator"), Publish]
        public Task<double> GetAccumulatorAsync(CancellationToken cancellationToken)
            => _sumService.GetAccumulatorAsync(cancellationToken);

        [HttpGet("sum"), Publish]
        public Task<double> SumAsync([FromQuery] double[] values, bool addAccumulator, CancellationToken cancellationToken)
            => _sumService.SumAsync(values, addAccumulator, cancellationToken);
    }
}
