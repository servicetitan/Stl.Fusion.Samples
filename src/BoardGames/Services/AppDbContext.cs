using Microsoft.EntityFrameworkCore;
using Stl.Fusion.EntityFramework.Authentication;
using Stl.Fusion.EntityFramework.Operations;

namespace Samples.BoardGames.Services
{
    public class AppDbContext : DbContext
    {
        public DbSet<DbGame> Games { get; protected set; } = null!;
        public DbSet<DbGamePlayer> GamePlayers { get; protected set; } = null!;

        // Stl.Fusion.EntityFramework tables
        public DbSet<DbOperation> Operations { get; protected set; } = null!;
        public DbSet<DbSessionInfo> Sessions { get; protected set; } = null!;
        public DbSet<DbUser> Users { get; protected set; } = null!;
        public DbSet<DbUserIdentity> UserIdentities { get; protected set; } = null!;

        public AppDbContext(DbContextOptions options) : base(options) { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            var dbGamePlayer = modelBuilder.Entity<DbGamePlayer>();
            dbGamePlayer.HasKey(p => new { p.GameId, p.UserId });
        }
    }
}
