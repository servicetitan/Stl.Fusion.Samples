using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.Extensions.DependencyInjection;

namespace Samples.Caching.Server.Services
{
    public static class AppDbContextExtensions
    {
        public static TDbContext RentDbContext<TDbContext>(this IServiceProvider services)
            where TDbContext : DbContext
#pragma warning disable EF1001
            => services.GetRequiredService<DbContextPool<TDbContext>>().Rent();
#pragma warning restore EF1001
    }
}
