using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Stl.DependencyInjection;

namespace Samples.Caching.Server.Services
{
    [Service]
    public class DbInitializer : DbServiceBase<AppDbContext>
    {
        public DbInitializer(IServiceProvider services) : base(services) { }

        public async Task InitializeAsync(bool recreate, CancellationToken cancellationToken = default)
        {
            // Ensure the DB is re-created
            await using var dbContext = Services.RentDbContext<AppDbContext>();
            if (recreate)
                await dbContext.Database.EnsureDeletedAsync();
            await dbContext.Database.EnsureCreatedAsync();
            await dbContext.Database.ExecuteSqlRawAsync(
                $"ALTER DATABASE {ServerSettings.DatabaseName} SET ALLOW_SNAPSHOT_ISOLATION ON");
            await dbContext.Database.ExecuteSqlRawAsync(
                $"ALTER DATABASE {ServerSettings.DatabaseName} SET RECOVERY SIMPLE WITH NO_WAIT");
        }
    }
}
