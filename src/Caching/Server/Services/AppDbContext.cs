using Microsoft.EntityFrameworkCore;
using Samples.Caching.Common;

namespace Samples.Caching.Server.Services
{
    public class AppDbContext : DbContext
    {
        public DbSet<Tenant> Tenants { get; protected set; } = null!;

        public AppDbContext(DbContextOptions options) : base(options)
        {
            // ReSharper disable once VirtualMemberCallInConstructor
            var ct = ChangeTracker;
            ct.AutoDetectChangesEnabled = false;
            ct.LazyLoadingEnabled = false;
            ct.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            var user = modelBuilder.Entity<Tenant>();
            user.Ignore(u => u.IsFrozen);
            user.HasKey(u => u.Id);
            user.HasIndex(u => u.Name);
            user.Property(u => u.Version).IsConcurrencyToken();
        }
    }
}
