namespace Samples.Benchmark.Server;

public class FusionTenants : DbTenants, IFusionTenants
{
    public FusionTenants(IServiceProvider services) : base(services) { }

    public override async Task AddOrUpdate(Tenant tenant, long? version, CancellationToken cancellationToken = default)
    {
        await base.AddOrUpdate(tenant, version, cancellationToken).ConfigureAwait(false);
        using (Computed.Invalidate()) {
            _ = TryGet(tenant.Id, default);
            _ = GetAll(default);
        }
    }

    public override async Task Remove(string tenantId, long version, CancellationToken cancellationToken = default)
    {
        await base.Remove(tenantId, version, cancellationToken).ConfigureAwait(false);
        using (Computed.Invalidate()) {
            _ = TryGet(tenantId, default);
            _ = GetAll(default);
        }
    }
}
