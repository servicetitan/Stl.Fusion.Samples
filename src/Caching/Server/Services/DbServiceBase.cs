using System;
using Microsoft.Extensions.DependencyInjection;
using Stl.Time;

namespace Samples.Caching.Server.Services
{
    public abstract class DbServiceBase<TDbContext>
        where TDbContext : ScopedDbContext
    {
        protected IServiceProvider Services { get; }
        protected IMomentClock Clock { get; }

        protected DbServiceBase(IServiceProvider services)
        {
            Services = services;
            Clock = services.GetRequiredService<IMomentClock>();
        }

        protected TDbContext RentDbContext()
            => Services.RentDbContext<TDbContext>();
    }
}
