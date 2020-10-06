using System.Threading;
using System.Threading.Tasks;
using RestEase;
using Stl.Fusion;
using Stl.Fusion.Client;

namespace Samples.Caching.Common
{
    public interface ITenantService
    {
        Task AddOrUpdate(Tenant tenant, long? version, CancellationToken cancellationToken = default);
        Task RemoveAsync(string tenantId, long? version, CancellationToken cancellationToken = default);

        // Compute methods
        [ComputeMethod(KeepAliveTime = 10)]
        Task<Tenant[]> GetAllAsync(CancellationToken cancellationToken = default);
        [ComputeMethod(KeepAliveTime = 10)]
        Task<Tenant> GetAsync(string tenantId, CancellationToken cancellationToken = default);
    }

    [RestEaseReplicaService(typeof(ITenantService))]
    [BasePath("tenants")]
    public interface ITenantClient
    {
        [Post("addOrUpdate")]
        Task AddOrUpdate(Tenant tenant, long? version, CancellationToken cancellationToken = default);
        [Post("remove")]
        Task RemoveAsync(string tenantId, long? version, CancellationToken cancellationToken = default);
        [Get("getAll")]
        Task<Tenant[]> GetAllAsync(CancellationToken cancellationToken = default);
        [Get("get")]
        Task<Tenant> GetAsync(string tenantId, CancellationToken cancellationToken = default);
    }
}
