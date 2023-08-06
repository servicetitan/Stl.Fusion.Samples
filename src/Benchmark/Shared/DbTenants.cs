using Microsoft.EntityFrameworkCore;
using Stl.Fusion.EntityFramework;

namespace Samples.Benchmark;

public class DbTenants : DbServiceBase<AppDbContext>, ITenants
{
    public DbTenants(IServiceProvider services) : base(services) { }

    public virtual async Task AddOrUpdate(Tenant tenant, long? version, CancellationToken cancellationToken = default)
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
    }

    public virtual async Task Remove(string tenantId, long version, CancellationToken cancellationToken = default)
    {
        await using var dbContext = CreateDbContext().ReadWrite();
        var entry = dbContext.Tenants.Remove(new Tenant() { Id = tenantId });
        entry.Property(nameof(Tenant.Version)).OriginalValue = version;
        await dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
    }

    // Compute methods

    public virtual async Task<Tenant[]> GetAll(CancellationToken cancellationToken = default)
    {
        await using var dbContext = CreateDbContext();
        var tenants = await dbContext.Tenants.AsQueryable()
            .ToArrayAsync(cancellationToken).ConfigureAwait(false);
        return tenants;
    }

    public virtual async Task<Tenant?> TryGet(string tenantId, CancellationToken cancellationToken = default)
    {
        // var c = Computed.GetExisting(() => GetAll(default));
        await using var dbContext = CreateDbContext();
        var tenant = await dbContext.Tenants.AsQueryable()
            .SingleOrDefaultAsync(t => t.Id == tenantId, cancellationToken).ConfigureAwait(false);
        return tenant;
    }
}
