using Microsoft.AspNetCore.Mvc;
using Samples.Caching.Common;
using Stl.Fusion.Server;

namespace Samples.Caching.Server.Controllers;

[Route("api/tenants/[action]")]
[ApiController, JsonifyErrors, UseDefaultSession]
public class TenantController : ControllerBase, ITenantService
{
    private readonly ITenantService _tenants;

    public TenantController(ITenantService tenants)
        => _tenants = tenants;

    [HttpPost]
    public Task AddOrUpdate([FromBody] Tenant tenant, long? version, CancellationToken cancellationToken = default)
        => _tenants.AddOrUpdate(tenant, version, cancellationToken);

    [HttpPost]
    public Task Remove(string tenantId, long version, CancellationToken cancellationToken = default)
        => _tenants.Remove(tenantId, version, cancellationToken);

    // Compute methods

    [HttpGet, Publish]
    public Task<Tenant[]> GetAll(CancellationToken cancellationToken = default)
        => _tenants.GetAll(cancellationToken);

    [HttpGet, Publish]
    public Task<Tenant?> Get(string tenantId, CancellationToken cancellationToken = default)
        => _tenants.Get(tenantId ?? "", cancellationToken);
}
