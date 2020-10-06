using System.Threading;
using System.Threading.Tasks;
using Stl.Fusion;

namespace Samples.Caching.Common
{
    public interface ITenantService
    {
        Task AddOrUpdateAsync(Tenant tenant, long? version, CancellationToken cancellationToken = default);
        Task RemoveAsync(string tenantId, long version, CancellationToken cancellationToken = default);

        // Compute methods
        [ComputeMethod(KeepAliveTime = 10)]
        Task<Tenant[]> GetAllAsync(CancellationToken cancellationToken = default);
        [ComputeMethod(KeepAliveTime = 10)]
        Task<Tenant?> TryGetAsync(string tenantId, CancellationToken cancellationToken = default);
    }

    public interface ISqlTenantService : ITenantService { }
}
