using Microsoft.EntityFrameworkCore;
using Samples.Blazor.Common.Services;
using Samples.Helpers;

namespace Samples.Blazor.Server.Services
{
    public class AppDbContext : ScopedDbContext
    {
        public DbSet<ChatUser> ChatUsers { get; protected set; } = null!;
        public DbSet<ChatMessage> ChatMessages { get; protected set; } = null!;

        public AppDbContext(DbContextOptions options) : base(options)
            => this.DisableChangeTracking();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            var user = modelBuilder.Entity<ChatUser>();
            user.HasIndex(u => u.Name);

            var message = modelBuilder.Entity<ChatMessage>();
            message.HasIndex(m => m.UserId);
            message.HasIndex(m => m.CreatedAt);
        }
    }
}
