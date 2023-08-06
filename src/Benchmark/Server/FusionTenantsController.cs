using Microsoft.AspNetCore.Mvc;

namespace Samples.Benchmark.Server;

[Route("api/fusionTenants/[action]")]
[ApiController]
public class FusionTenantsController : Controller, ITenants
{
    private IFusionTenants Service { get; }

    public FusionTenantsController(IFusionTenants service)
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
