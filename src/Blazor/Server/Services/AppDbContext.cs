using Microsoft.EntityFrameworkCore;
using Samples.Blazor.Abstractions;
using Stl.Fusion.EntityFramework;

namespace Samples.Blazor.Server.Services
{
    public class AppDbContext : DbContext
    {
        public DbSet<ChatUser> ChatUsers { get; protected set; } = null!;
        public DbSet<ChatMessage> ChatMessages { get; protected set; } = null!;
        public DbSet<DbOperation> Operations { get; protected set; } = null!;

        public AppDbContext(DbContextOptions options) : base(options) { }
    }
}
