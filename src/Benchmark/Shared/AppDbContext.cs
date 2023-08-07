using Microsoft.EntityFrameworkCore;

namespace Samples.Benchmark;

public class AppDbContext : DbContext
{
    public DbSet<TestItem> Tenants { get; protected set; } = null!;

    public AppDbContext(DbContextOptions options) : base(options) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        var user = modelBuilder.Entity<TestItem>();
        user.HasKey(u => u.Id);
        user.HasIndex(u => u.Name);
        user.Property(u => u.Version).IsConcurrencyToken();
    }
}
