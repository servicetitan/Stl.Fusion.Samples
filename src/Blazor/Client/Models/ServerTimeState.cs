using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Stl.Fusion.UI;
using Samples.Blazor.Common.Services;

namespace Samples.Blazor.Client.Models
{
    public class ServerTimeState
    {
        public DateTime? Time { get; set; }

        [LiveStateUpdater]
        public class Updater : ILiveStateUpdater<ServerTimeState>
        {
            protected ITimeService Time { get; }

            public Updater(ITimeService time) => Time = time;

            public virtual async Task<ServerTimeState> UpdateAsync(
                ILiveState<ServerTimeState> liveState, CancellationToken cancellationToken)
            {
                Debug.WriteLine($"{DateTime.Now.ToString("hh:mm:ss.fff")}: @ {GetType().Name}.{nameof(UpdateAsync)}");
                var time = await Time.GetTimeAsync(cancellationToken);
                return new ServerTimeState() { Time = time };
            }
        }
    }
}
