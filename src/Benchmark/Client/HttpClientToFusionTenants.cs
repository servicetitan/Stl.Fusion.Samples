using RestEase;

namespace Samples.Benchmark.Client;

[BasePath("api/fusionTenants")]
public interface IFusionTenantsClientDef
{
    [Post(nameof(AddOrUpdate))]
    Task AddOrUpdate([Body] Tenant tenant, long? version, CancellationToken cancellationToken = default);
    [Post(nameof(Remove))]
    Task Remove(string tenantId, long version, CancellationToken cancellationToken = default);

    [Get(nameof(GetAll))]
    Task<Tenant[]> GetAll(CancellationToken cancellationToken = default);
    [Get(nameof(TryGet))]
    Task<Tenant?> TryGet(string tenantId, CancellationToken cancellationToken = default);
}

public class HttpClientToFusionTenants : ITenants
{
    private readonly IFusionTenantsClientDef _client;

    public HttpClientToFusionTenants(IServiceProvider services)
        => _client = services.GetRequiredService<IFusionTenantsClientDef>();

    public Task AddOrUpdate(Tenant tenant, long? version, CancellationToken cancellationToken = default)
        => _client.AddOrUpdate(tenant, version, cancellationToken);

    public Task Remove(string tenantId, long version, CancellationToken cancellationToken = default)
        => _client.Remove(tenantId, version, cancellationToken);

    public Task<Tenant[]> GetAll(CancellationToken cancellationToken = default)
        => _client.GetAll(cancellationToken);

    public Task<Tenant?> TryGet(string tenantId, CancellationToken cancellationToken = default)
        => _client.TryGet(tenantId, cancellationToken);
}
