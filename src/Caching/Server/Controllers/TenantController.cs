using Microsoft.AspNetCore.Mvc;
using Samples.Caching.Common;
using Stl.Fusion.Server;

namespace Samples.Caching.Server.Controllers;

[Route("api/tenants/[action]")]
[ApiController, JsonifyErrors]
public class TenantController : ControllerBase, ITenantService
{
    private ITenantService Tenants { get; }

    public TenantController(ITenantService tenants)
        => Tenants = tenants;

    [HttpPost]
    public Task AddOrUpdate([FromBody] Tenant tenant, long? version, CancellationToken cancellationToken = default)
        => Tenants.AddOrUpdate(tenant, version, cancellationToken);

    [HttpPost]
    public Task Remove(string tenantId, long version, CancellationToken cancellationToken = default)
        => Tenants.Remove(tenantId, version, cancellationToken);

    // Compute methods

    [HttpGet, Publish]
    public Task<Tenant[]> GetAll(CancellationToken cancellationToken = default)
        => Tenants.GetAll(cancellationToken);

    [HttpGet, Publish]
    public Task<Tenant?> TryGet(string tenantId, CancellationToken cancellationToken = default)
        => Tenants.TryGet(tenantId ?? "", cancellationToken);
}
