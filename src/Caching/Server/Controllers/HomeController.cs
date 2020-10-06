using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Samples.Caching.Client;
using Stl.Fusion.Bridge;
using Stl.Fusion.Server;

namespace Samples.Caching.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TenantController : FusionController, ITenantService
    {
        private ITenantService Tenants { get; }

        public TenantController(ITenantService chat, IPublisher publisher)
            : base(publisher)
            => Tenants = chat;

        [HttpPost("addOrUpdate")]
        public Task AddOrUpdate(Tenant tenant, long? version, CancellationToken cancellationToken = default)
            => Tenants.AddOrUpdate(tenant, version, cancellationToken);

        [HttpPost("remove")]
        public Task RemoveAsync(string tenantId, long? version, CancellationToken cancellationToken = default)
            => Tenants.RemoveAsync(tenantId, version, cancellationToken);

        // Compute methods

        [HttpPost("getAll")]
        public Task<Tenant[]> GetAllAsync(CancellationToken cancellationToken = default)
            => PublishAsync(ct => Tenants.GetAllAsync(ct), cancellationToken);

        [HttpPost("get")]
        public Task<Tenant> GetAsync(string tenantId, CancellationToken cancellationToken = default)
            => PublishAsync(ct => Tenants.GetAsync(tenantId ?? "", ct), cancellationToken);
    }
}
