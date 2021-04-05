using System.Threading;
using System.Threading.Tasks;
using RestEase;
using Samples.Caching.Common;
using Stl.Fusion.Client;

namespace Samples.Caching.Client
{
    [RestEaseReplicaService(typeof(ITenantService), Scope = Program.ClientSideScope)]
    [RestEaseClientService(typeof(IRestEaseTenantService), Scope = Program.ClientSideScope)]
    [BasePath("tenants")]
    public interface ITenantClient
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

    public interface IRestEaseTenantService : ITenantService { }

    [RestEaseClientService(typeof(IRestEaseTenantService), Scope = Program.ClientSideScope)]
    [BasePath("tenants")]
    public interface IRestEaseTenantClient
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

    [RestEaseClientService(typeof(ISqlTenantService), Scope = Program.ClientSideScope)]
    [BasePath("sqlTenants")]
    public interface ISqlTenantClient
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
}
