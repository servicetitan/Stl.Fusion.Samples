using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Samples.Caching.Client;
using Stl.Fusion;

namespace Samples.Caching.Server.Services
{
    public class TenantService : DbServiceBase<AppDbContext>, ITenantService
    {
        public TenantService(IServiceProvider services) : base(services) { }

        public async Task CreateOrUpdateAsync(Tenant tenant, long? version, CancellationToken cancellationToken = default)
        {
            await using var dbContext = RentDbContext();
            var entry = dbContext.Tenants.Add(tenant);
            if (version.HasValue)
                entry.Property(nameof(Tenant.Version)).OriginalValue = version.GetValueOrDefault();
            await dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

            // Invalidation
            Computed.Invalidate(() => GetAsync(tenant.Id, CancellationToken.None));
            Computed.Invalidate(() => GetAllAsync(CancellationToken.None));
        }

        public async Task DeleteAsync(string tenantId, long? version, CancellationToken cancellationToken = default)
        {
            var tenant = await GetAsync(tenantId, cancellationToken).ConfigureAwait(false);

            await using var dbContext = RentDbContext();
            var entry = dbContext.Tenants.Attach(tenant);
            if (version.HasValue)
                entry.Property(nameof(Tenant.Version)).OriginalValue = version.GetValueOrDefault();
            await dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

            // Invalidation
            Computed.Invalidate(() => GetAsync(tenant.Id, CancellationToken.None));
            Computed.Invalidate(() => GetAllAsync(CancellationToken.None));
        }

        public async Task<Tenant[]> GetAllAsync(CancellationToken cancellationToken = default)
        {
            await using var dbContext = RentDbContext();
            return await dbContext.Tenants.ToArrayAsync(cancellationToken).ConfigureAwait(false);
        }

        public async Task<Tenant> GetAsync(string tenantId, CancellationToken cancellationToken = default)
        {
            // var c = Computed.TryGetExisting(() => GetAllAsync(default));
            await using var dbContext = RentDbContext();
            return await dbContext.Tenants.SingleAsync(cancellationToken).ConfigureAwait(false);
        }
    }
}
