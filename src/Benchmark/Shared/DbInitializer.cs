using Microsoft.EntityFrameworkCore;
using Stl.Fusion.EntityFramework;

namespace Samples.Benchmark;

public class DbInitializer : DbServiceBase<AppDbContext>
{
    public DbInitializer(IServiceProvider services) : base(services) { }

    public async Task Initialize(bool recreate, CancellationToken cancellationToken = default)
    {
        // Ensure the DB is re-created
        var dbContextFactory = Services.GetRequiredService<IDbContextFactory<AppDbContext>>();
        await using var dbContext = await dbContextFactory.CreateDbContextAsync(cancellationToken);
        if (recreate)
            await dbContext.Database.EnsureDeletedAsync(cancellationToken);
        await dbContext.Database.EnsureCreatedAsync(cancellationToken);
    }
}
