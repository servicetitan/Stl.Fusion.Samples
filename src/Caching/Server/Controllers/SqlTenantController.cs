using Microsoft.AspNetCore.Mvc;
using Samples.Caching.Common;

namespace Samples.Caching.Server.Controllers;

[Route("api/sqlTenants/[action]")]
[ApiController]
public class SqlTenantController : Controller, ISqlTenantService
{
    private ISqlTenantService Tenants { get; }

    public SqlTenantController(ISqlTenantService tenants)
        => Tenants = tenants;

    [HttpPost]
    public Task AddOrUpdate([FromBody] Tenant tenant, long? version, CancellationToken cancellationToken = default)
        => Tenants.AddOrUpdate(tenant, version, cancellationToken);

    [HttpPost]
    public Task Remove(string tenantId, long version, CancellationToken cancellationToken = default)
        => Tenants.Remove(tenantId, version, cancellationToken);

    // Compute methods

    [HttpGet]
    public Task<Tenant[]> GetAll(CancellationToken cancellationToken = default)
        => Tenants.GetAll(cancellationToken);

    [HttpGet]
    public Task<Tenant?> Get(string tenantId, CancellationToken cancellationToken = default)
        => Tenants.Get(tenantId ?? "", cancellationToken);
}
