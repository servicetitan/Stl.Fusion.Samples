using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Samples.Caching.Common;
using Stl.Async;
using Stl.Fusion;
using Stl.Fusion.EntityFramework;

namespace Samples.Caching.Server.Services
{
    public class TenantService : DbServiceBase<AppDbContext>, ISqlTenantService
    {
        private bool IsComputeService { get; }

        public TenantService(IServiceProvider services) : base(services)
            => IsComputeService = GetType() != typeof(TenantService);

        public async Task AddOrUpdate(Tenant tenant, long? version, CancellationToken cancellationToken = default)
        {
            await using var dbContext = CreateDbContext().ReadWrite();
            if (version.HasValue) {
                var entry = dbContext.Tenants.Update(tenant);
                entry.Property(nameof(Tenant.Version)).OriginalValue = version.GetValueOrDefault();
            }
            else {
                dbContext.Tenants.Add(tenant);
            }
            await dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

            if (IsComputeService)
                using (Computed.Invalidate()) {
                    TryGet(tenant.Id, default).Ignore();
                    GetAll(default).Ignore();
                }
        }

        public async Task Remove(string tenantId, long version, CancellationToken cancellationToken = default)
        {
            await using var dbContext = CreateDbContext().ReadWrite();
            var entry = dbContext.Tenants.Remove(new Tenant() { Id = tenantId });
            entry.Property(nameof(Tenant.Version)).OriginalValue = version;
            await dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

            if (IsComputeService)
                using (Computed.Invalidate()) {
                    TryGet(tenantId, default).Ignore();
                    GetAll(default).Ignore();
                }
        }

        // Compute methods

        public virtual async Task<Tenant[]> GetAll(CancellationToken cancellationToken = default)
        {
            await using var dbContext = CreateDbContext();
            var tenants = await dbContext.Tenants.ToArrayAsync(cancellationToken).ConfigureAwait(false);
            return tenants;
        }

        public virtual async Task<Tenant?> TryGet(string tenantId, CancellationToken cancellationToken = default)
        {
            // var c = Computed.TryGetExisting(() => GetAllAsync(default));
            await using var dbContext = CreateDbContext();
            var tenant = await dbContext.Tenants
                .SingleOrDefaultAsync(t => t.Id == tenantId, cancellationToken).ConfigureAwait(false);
            return tenant;
        }
    }
}
