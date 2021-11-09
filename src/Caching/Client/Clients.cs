using System.Threading;
using System.Threading.Tasks;
using RestEase;
using Samples.Caching.Common;

namespace Samples.Caching.Client;

[BasePath("tenants")]
public interface ITenantClientDef
{
    [Post("addOrUpdate")]
    Task AddOrUpdate([Body] Tenant tenant, long? version, CancellationToken cancellationToken = default);
    [Post("remove")]
    Task Remove(string tenantId, long version, CancellationToken cancellationToken = default);
    [Get("getAll")]
    Task<Tenant[]> GetAll(CancellationToken cancellationToken = default);
    [Get("tryGet")]
    Task<Tenant> TryGet(string tenantId, CancellationToken cancellationToken = default);
}

[BasePath("sqlTenants")]
public interface ISqlTenantClientDef
{
    [Post("addOrUpdate")]
    Task AddOrUpdate([Body] Tenant tenant, long? version, CancellationToken cancellationToken = default);
    [Post("remove")]
    Task Remove(string tenantId, long version, CancellationToken cancellationToken = default);
    [Get("getAll")]
    Task<Tenant[]> GetAll(CancellationToken cancellationToken = default);
    [Get("tryGet")]
    Task<Tenant> TryGet(string tenantId, CancellationToken cancellationToken = default);
}
