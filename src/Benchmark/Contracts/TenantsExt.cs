namespace Samples.Benchmark;

public static class TenantsExt
{
    public static async Task<Tenant> Get(this ITenants tenants,
        string tenantId, CancellationToken cancellationToken = default)
    {
        var tenant = await tenants.TryGet(tenantId, cancellationToken).ConfigureAwait(false);
        return tenant ?? throw new KeyNotFoundException();
    }
}
