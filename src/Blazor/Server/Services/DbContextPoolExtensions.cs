using System;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.Extensions.DependencyInjection;

namespace Samples.Blazor.Server.Services
{
    public static class AppDbContextExtensions
    {
        public static AppDbContext RentDbContext(this IServiceProvider services)
#pragma warning disable EF1001
            => services.GetRequiredService<DbContextPool<AppDbContext>>().Rent();
#pragma warning restore EF1001
    }
}
