using System.Collections.Generic;

namespace Samples.Caching.Common;

public static class TenantServiceEx
{
    public static async Task<Tenant> Get(this ITenantService tenants,
        string tenantId, CancellationToken cancellationToken = default)
    {
        var tenant = await tenants.Get(tenantId, cancellationToken).ConfigureAwait(false);
        return tenant ?? throw new KeyNotFoundException();
    }
}
