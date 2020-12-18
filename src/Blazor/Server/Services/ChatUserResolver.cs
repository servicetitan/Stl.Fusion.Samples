using System;
using Samples.Blazor.Abstractions;
using Samples.Helpers;
using Stl.DependencyInjection;

namespace Samples.Blazor.Server.Services
{
    [Service]
    public class ChatUserResolver : DbEntityResolver<AppDbContext, long, ChatUser>
    {
        public ChatUserResolver(IServiceProvider services) : base(services)
        {
            BatchProcessor.ConcurrencyLevel = 1; // Just to show how it works
        }
    }
}
