using System.Threading;
using System.Threading.Tasks;
using Microsoft.OpenApi.Writers;
using RestEase;
using Samples.Caching.Common;
using Stl.Fusion.Client;
using Stl.Fusion.Client.RestEase;

namespace Samples.Caching.Client
{
    [RestEaseReplicaService(typeof(ITenantService), Scope = Program.ClientSideScope)]
    [RestEaseClientService(typeof(IRestEaseTenantService), Scope = Program.ClientSideScope)]
    [BasePath("tenants")]
    public interface ITenantClient : IRestEaseReplicaClient
    {
        [Post("addOrUpdate")]
        Task AddOrUpdateAsync([Body] Tenant tenant, long? version, CancellationToken cancellationToken = default);
        [Post("remove")]
        Task RemoveAsync(string tenantId, long version, CancellationToken cancellationToken = default);
        [Get("getAll")]
        Task<Tenant[]> GetAllAsync(CancellationToken cancellationToken = default);
        [Get("get")]
        Task<Tenant> TryGetAsync(string tenantId, CancellationToken cancellationToken = default);
    }

    public interface IRestEaseTenantService : ITenantService { }

    [RestEaseClientService(typeof(IRestEaseTenantService), Scope = Program.ClientSideScope)]
    [BasePath("tenants")]
    public interface IRestEaseTenantClient
    {
        [Post("addOrUpdate")]
        Task AddOrUpdateAsync([Body] Tenant tenant, long? version, CancellationToken cancellationToken = default);
        [Post("remove")]
        Task RemoveAsync(string tenantId, long version, CancellationToken cancellationToken = default);
        [Get("getAll")]
        Task<Tenant[]> GetAllAsync(CancellationToken cancellationToken = default);
        [Get("get")]
        Task<Tenant> TryGetAsync(string tenantId, CancellationToken cancellationToken = default);
    }

    [RestEaseClientService(typeof(ISqlTenantService), Scope = Program.ClientSideScope)]
    [BasePath("sqlTenants")]
    public interface ISqlTenantClient
    {
        [Post("addOrUpdate")]
        Task AddOrUpdateAsync([Body] Tenant tenant, long? version, CancellationToken cancellationToken = default);
        [Post("remove")]
        Task RemoveAsync(string tenantId, long version, CancellationToken cancellationToken = default);
        [Get("getAll")]
        Task<Tenant[]> GetAllAsync(CancellationToken cancellationToken = default);
        [Get("get")]
        Task<Tenant> TryGetAsync(string tenantId, CancellationToken cancellationToken = default);
    }
}
