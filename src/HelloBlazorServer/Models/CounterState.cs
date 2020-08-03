using System.Threading;
using System.Threading.Tasks;
using Samples.HelloBlazorServer.Services;
using Stl.Fusion.UI;

namespace Samples.HelloBlazorServer.Models
{
    public class CounterState
    {
        public int Counter { get; set; }

        [LiveStateUpdater]
        public class Updater : ILiveStateUpdater<CounterState>
        {
            private CounterService CounterService { get; }

            public Updater(CounterService counterService)
                => CounterService = counterService;

            public async Task<CounterState> UpdateAsync(
                ILiveState<CounterState> liveState, CancellationToken cancellationToken)
            {
                var counter = await CounterService.GetCounterAsync(cancellationToken);
                return new CounterState() {
                    Counter = counter
                };
            }
        }
    }
}
