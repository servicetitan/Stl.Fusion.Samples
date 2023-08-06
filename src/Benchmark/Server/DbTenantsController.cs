using Microsoft.AspNetCore.Mvc;

namespace Samples.Benchmark.Server;

[Route("api/dbTenants/[action]")]
[ApiController]
public class DbTenantsController : Controller, ITenants
{
    private DbTenants Service { get; }

    public DbTenantsController(DbTenants service)
        => Service = service;

    [HttpPost]
    public Task AddOrUpdate([FromBody] Tenant tenant, long? version, CancellationToken cancellationToken = default)
        => Service.AddOrUpdate(tenant, version, cancellationToken);

    [HttpPost]
    public Task Remove(string? tenantId, long version, CancellationToken cancellationToken = default)
        => Service.Remove(tenantId ?? "", version, cancellationToken);

    // Compute methods

    [HttpGet]
    public Task<Tenant[]> GetAll(CancellationToken cancellationToken = default)
        => Service.GetAll(cancellationToken);

    [HttpGet]
    public Task<Tenant?> TryGet(string? tenantId, CancellationToken cancellationToken = default)
        => Service.TryGet(tenantId ?? "", cancellationToken);
}
