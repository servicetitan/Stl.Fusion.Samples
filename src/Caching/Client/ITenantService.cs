using System.Threading;
using System.Threading.Tasks;
using Stl.Fusion;

namespace Samples.Caching.Client
{
    public interface ITenantService
    {
        Task CreateOrUpdateAsync(Tenant tenant, long? version, CancellationToken cancellationToken = default);
        Task DeleteAsync(string tenantId, long? version, CancellationToken cancellationToken = default);

        // Compute methods
        [ComputeMethod(KeepAliveTime = 10)]
        Task<Tenant[]> GetAllAsync(CancellationToken cancellationToken = default);
        [ComputeMethod(KeepAliveTime = 10)]
        Task<Tenant> GetAsync(string tenantId, CancellationToken cancellationToken = default);
    }
}
