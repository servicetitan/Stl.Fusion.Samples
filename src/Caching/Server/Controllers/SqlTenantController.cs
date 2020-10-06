using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Samples.Caching.Common;

namespace Samples.Caching.Server.Controllers
{
    [Route("api/sqlTenants")]
    [ApiController]
    public class SqlTenantController : Controller, ISqlTenantService
    {
        private ISqlTenantService Tenants { get; }

        public SqlTenantController(ISqlTenantService tenants)
            => Tenants = tenants;

        [HttpPost("addOrUpdate")]
        public Task AddOrUpdateAsync([FromBody] Tenant tenant, long? version, CancellationToken cancellationToken = default)
            => Tenants.AddOrUpdateAsync(tenant, version, cancellationToken);

        [HttpPost("remove")]
        public Task RemoveAsync(string tenantId, long version, CancellationToken cancellationToken = default)
            => Tenants.RemoveAsync(tenantId, version, cancellationToken);

        // Compute methods

        [HttpGet("getAll")]
        public Task<Tenant[]> GetAllAsync(CancellationToken cancellationToken = default)
            => Tenants.GetAllAsync(cancellationToken);

        [HttpGet("get")]
        public Task<Tenant?> TryGetAsync(string tenantId, CancellationToken cancellationToken = default)
            => Tenants.TryGetAsync(tenantId ?? "", cancellationToken);
    }
}
