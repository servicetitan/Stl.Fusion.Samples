using Microsoft.AspNetCore.DataProtection.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Samples.Blazor.Abstractions;
using Stl.Fusion.Authentication.Services;
using Stl.Fusion.EntityFramework.Operations;

namespace Samples.Blazor.Server.Services;

public class AppDbContext : DbContext, IDataProtectionKeyContext
{
    public DbSet<ChatMessage> ChatMessages { get; protected set; } = null!;

    // Stl.Fusion.EntityFramework tables
    public DbSet<DbOperation> Operations { get; protected set; } = null!;
    public DbSet<DbSessionInfo<long>> Sessions { get; protected set; } = null!;
    public DbSet<DbUser<long>> Users { get; protected set; } = null!;
    public DbSet<DbUserIdentity<long>> UserIdentities { get; protected set; } = null!;

    // Data protection key storage
    public DbSet<DataProtectionKey> DataProtectionKeys { get; protected set; } = null!;

    public AppDbContext(DbContextOptions options) : base(options) { }
}
