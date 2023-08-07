using Microsoft.EntityFrameworkCore;
using Stl.Fusion.EntityFramework;

namespace Samples.Benchmark;

public class DbTestService : DbServiceBase<AppDbContext>, ITestService
{
    public DbTestService(IServiceProvider services) : base(services) { }

    public virtual async Task AddOrUpdate(TestItem item, long? version, CancellationToken cancellationToken = default)
    {
        if (item.Id == 0)
            throw new ArgumentOutOfRangeException(nameof(item));

        await using var dbContext = CreateDbContext().ReadWrite();
        if (version.HasValue) {
            var entry = dbContext.Tenants.Update(item);
            entry.Property(nameof(TestItem.Version)).OriginalValue = version.GetValueOrDefault();
        }
        else
            dbContext.Tenants.Add(item);

        await dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
    }

    public virtual async Task Remove(long itemId, long version, CancellationToken cancellationToken = default)
    {
        if (itemId == 0)
            throw new ArgumentOutOfRangeException(nameof(itemId));

        await using var dbContext = CreateDbContext().ReadWrite();
        var entry = dbContext.Tenants.Remove(new TestItem() { Id = itemId });
        entry.Property(nameof(TestItem.Version)).OriginalValue = version;
        await dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
    }

    // Compute methods

    public virtual async Task<TestItem[]> GetAll(CancellationToken cancellationToken = default)
    {
        await using var dbContext = CreateDbContext();
        var tenants = await dbContext.Tenants.AsQueryable()
            .ToArrayAsync(cancellationToken).ConfigureAwait(false);
        return tenants;
    }

    public virtual async Task<TestItem?> TryGet(long itemId, CancellationToken cancellationToken = default)
    {
        // var c = Computed.GetExisting(() => GetAll(default));
        await using var dbContext = CreateDbContext();
        var tenant = await dbContext.Tenants.AsQueryable()
            .SingleOrDefaultAsync(t => t.Id == itemId, cancellationToken).ConfigureAwait(false);
        return tenant;
    }
}
