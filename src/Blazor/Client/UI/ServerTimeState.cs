using System;
using System.Threading;
using System.Threading.Tasks;
using Stl.Fusion.UI;
using Samples.Blazor.Client.Services;

namespace Samples.Blazor.Client.UI
{
    public class ServerTimeState
    {
        public DateTime? Time { get; set; }

        public class Updater : ILiveStateUpdater<ServerTimeState>
        {
            protected ITimeClient Client { get; }

            public Updater(ITimeClient client) => Client = client;

            public virtual async Task<ServerTimeState> UpdateAsync(
                ILiveState<ServerTimeState> liveState, CancellationToken cancellationToken)
            {
                var time = await Client.GetTimeAsync(cancellationToken);
                return new ServerTimeState() { Time = time };
            }
        }
    }
}
