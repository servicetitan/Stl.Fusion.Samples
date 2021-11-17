using RestEase;
using Samples.Caching.Common;

namespace Samples.Caching.Client;

[BasePath("tenants")]
public interface ITenantClientDef
{
    [Post(nameof(AddOrUpdate))]
    Task AddOrUpdate([Body] Tenant tenant, long? version, CancellationToken cancellationToken = default);
    [Post(nameof(Remove))]
    Task Remove(string tenantId, long version, CancellationToken cancellationToken = default);
    [Get(nameof(GetAll))]
    Task<Tenant[]> GetAll(CancellationToken cancellationToken = default);
    [Get(nameof(Get))]
    Task<Tenant> Get(string tenantId, CancellationToken cancellationToken = default);
}

[BasePath("sqlTenants")]
public interface ISqlTenantClientDef
{
    [Post(nameof(AddOrUpdate))]
    Task AddOrUpdate([Body] Tenant tenant, long? version, CancellationToken cancellationToken = default);
    [Post(nameof(Remove))]
    Task Remove(string tenantId, long version, CancellationToken cancellationToken = default);
    [Get(nameof(GetAll))]
    Task<Tenant[]> GetAll(CancellationToken cancellationToken = default);
    [Get(nameof(Get))]
    Task<Tenant> Get(string tenantId, CancellationToken cancellationToken = default);
}
