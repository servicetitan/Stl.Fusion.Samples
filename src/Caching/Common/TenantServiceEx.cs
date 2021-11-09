using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Samples.Caching.Common;

public static class TenantServiceEx
{
    public static async Task<Tenant> Get(this ITenantService tenants,
        string tenantId, CancellationToken cancellationToken = default)
    {
        var tenant = await tenants.TryGet(tenantId, cancellationToken).ConfigureAwait(false);
        return tenant ?? throw new KeyNotFoundException();
    }
}
