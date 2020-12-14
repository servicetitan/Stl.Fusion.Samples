using Microsoft.EntityFrameworkCore;
using Templates.Blazor.Common.Services;
using Samples.Helpers;

namespace Templates.Blazor.Server.Services
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions options) : base(options)
            => this.DisableChangeTracking();
    }
}
