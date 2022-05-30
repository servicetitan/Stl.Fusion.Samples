using Microsoft.EntityFrameworkCore;
using Stl.Fusion.EntityFramework;

namespace Samples.Caching.Server.Services;

public class DbInitializer : DbServiceBase<AppDbContext>
{
    public DbInitializer(IServiceProvider services) : base(services) { }

    public async Task Initialize(bool recreate, CancellationToken cancellationToken = default)
    {
        // Ensure the DB is re-created
        var dbSettings = Services.GetRequiredService<DbSettings>();
        var dbContextFactory = Services.GetRequiredService<IDbContextFactory<AppDbContext>>();
        await using var dbContext = dbContextFactory.CreateDbContext();
        if (recreate)
            await dbContext.Database.EnsureDeletedAsync(cancellationToken);
        await dbContext.Database.EnsureCreatedAsync(cancellationToken);
        await dbContext.Database.ExecuteSqlRawAsync(
            $"ALTER DATABASE {dbSettings.DatabaseName} SET ALLOW_SNAPSHOT_ISOLATION ON", cancellationToken);
        await dbContext.Database.ExecuteSqlRawAsync(
            $"ALTER DATABASE {dbSettings.DatabaseName} SET RECOVERY SIMPLE WITH NO_WAIT", cancellationToken);
    }
}
