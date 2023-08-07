using Microsoft.EntityFrameworkCore;
using Stl.Fusion.EntityFramework;

namespace Samples.Benchmark;

public static class ServiceCollectionExt
{
    public static IServiceCollection AddAppDbContext(this IServiceCollection services)
    {
        services.AddPooledDbContextFactory<AppDbContext>((_, dbContext) => {
            dbContext.UseNpgsql(Settings.DbConnectionString, _ => { });
        }, 512);
        services.AddDbContextServices<AppDbContext>();
        services.AddSingleton<DbInitializer>();
        services.AddSingleton<DbTestService>();
        return services;
    }
}
