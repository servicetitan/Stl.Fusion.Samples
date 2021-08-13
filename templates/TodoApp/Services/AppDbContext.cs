using Microsoft.EntityFrameworkCore;
using Stl.Fusion.EntityFramework.Authentication;
using Stl.Fusion.EntityFramework.Extensions;
using Stl.Fusion.EntityFramework.Operations;

namespace Templates.TodoApp.Services
{
    public class AppDbContext : DbContext
    {
        // Stl.Fusion.EntityFramework tables
        public DbSet<DbUser<long>> Users { get; protected set; } = null!;
        public DbSet<DbUserIdentity<long>> UserIdentities { get; protected set; } = null!;
        public DbSet<DbSessionInfo<long>> Sessions { get; protected set; } = null!;
        public DbSet<DbKeyValue> KeyValues { get; protected set; } = null!;
        public DbSet<DbOperation> Operations { get; protected set; } = null!;

        public AppDbContext(DbContextOptions options) : base(options) { }
    }
}
