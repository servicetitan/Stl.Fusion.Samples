using Stl.Rpc;

namespace Samples.Benchmark;

public interface ITenants
{
    Task AddOrUpdate(Tenant tenant, long? version, CancellationToken cancellationToken = default);
    Task Remove(string tenantId, long version, CancellationToken cancellationToken = default);

    // Compute methods
    [ComputeMethod]
    Task<Tenant[]> GetAll(CancellationToken cancellationToken = default);
    [ComputeMethod]
    Task<Tenant?> TryGet(string tenantId, CancellationToken cancellationToken = default);
}

public interface IRpcTenants : ITenants, IRpcService
{ }

public interface IFusionTenants : IRpcTenants, IComputeService
{ }
