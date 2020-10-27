using System;
using Microsoft.Extensions.DependencyInjection;

namespace Samples.Helpers
{
    public static class ScopedDbContextEx
    {
        public static TDbContext RentDbContext<TDbContext>(this IServiceProvider serviceProvider)
            where TDbContext : ScopedDbContext
        {
            var scope = serviceProvider.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<TDbContext>();
            dbContext.Scope = scope;
            return dbContext;
        }
    }
}
