using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Samples.Caching.Common;
using Samples.Helpers;
using Stl.DependencyInjection;
using Stl.Frozen;
using Stl.Fusion;

namespace Samples.Caching.Server.Services
{
    [ComputeService(typeof(ITenantService))]
    [Service(typeof(ISqlTenantService))]
    public class TenantService : DbServiceBase<AppDbContext>, ISqlTenantService
    {
        private bool IsComputeService { get; }

        public TenantService(IServiceProvider services) : base(services)
            => IsComputeService = GetType() != typeof(TenantService);

        public async Task AddOrUpdateAsync(Tenant tenant, long? version, CancellationToken cancellationToken = default)
        {
            await using var dbContext = CreateDbContext();
            if (version.HasValue) {
                var entry = dbContext.Tenants.Update(tenant);
                entry.Property(nameof(Tenant.Version)).OriginalValue = version.GetValueOrDefault();
            }
            else {
                dbContext.Tenants.Add(tenant);
            }
            await dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

            if (IsComputeService) {
                Computed.Invalidate(() => TryGetAsync(tenant.Id, CancellationToken.None));
                Computed.Invalidate(() => GetAllAsync(CancellationToken.None));
            }
        }

        public async Task RemoveAsync(string tenantId, long version, CancellationToken cancellationToken = default)
        {
            await using var dbContext = CreateDbContext();
            var entry = dbContext.Tenants.Remove(new Tenant() { Id = tenantId });
            entry.Property(nameof(Tenant.Version)).OriginalValue = version;
            await dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

            if (IsComputeService) {
                Computed.Invalidate(() => TryGetAsync(tenantId, CancellationToken.None));
                Computed.Invalidate(() => GetAllAsync(CancellationToken.None));
            }
        }

        // Compute methods

        public virtual async Task<Tenant[]> GetAllAsync(CancellationToken cancellationToken = default)
        {
            await using var dbContext = CreateDbContext();
            var tenants = await dbContext.Tenants.ToArrayAsync(cancellationToken).ConfigureAwait(false);
            return tenants;
        }

        public virtual async Task<Tenant?> TryGetAsync(string tenantId, CancellationToken cancellationToken = default)
        {
            // var c = Computed.TryGetExisting(() => GetAllAsync(default));
            await using var dbContext = CreateDbContext();
            var tenant = await dbContext.Tenants
                .SingleOrDefaultAsync(t => t.Id == tenantId, cancellationToken).ConfigureAwait(false);
            return tenant;
        }
    }
}
