using Microsoft.EntityFrameworkCore;

namespace Samples.Benchmark;

public class AppDbContext : DbContext
{
    public DbSet<Tenant> Tenants { get; protected set; } = null!;

    public AppDbContext(DbContextOptions options) : base(options) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        var user = modelBuilder.Entity<Tenant>();
        user.HasKey(u => u.Id);
        user.HasIndex(u => u.Name);
        user.Property(u => u.Version).IsConcurrencyToken();
    }
}
