using System;
using Templates.Blazor.Common.Services;
using Samples.Helpers;
using Stl.DependencyInjection;

namespace Templates.Blazor.Server.Services
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
