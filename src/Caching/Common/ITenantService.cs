using System.Threading;
using System.Threading.Tasks;
using Stl.Fusion;

namespace Samples.Caching.Common;

public interface ITenantService
{
    Task AddOrUpdate(Tenant tenant, long? version, CancellationToken cancellationToken = default);
    Task Remove(string tenantId, long version, CancellationToken cancellationToken = default);

    // Compute methods
    [ComputeMethod(KeepAliveTime = 10)]
    Task<Tenant[]> GetAll(CancellationToken cancellationToken = default);
    [ComputeMethod(KeepAliveTime = 10)]
    Task<Tenant?> TryGet(string tenantId, CancellationToken cancellationToken = default);
}

public interface ISqlTenantService : ITenantService { }
