using System;
using System.Threading;
using System.Threading.Tasks;
using Stl.Fusion.UI;
using Samples.Blazor.Common.Services;

namespace Samples.Blazor.Client.Models
{
    public class ServerScreenState
    {
        public Screenshot Screenshot { get; set; } = new Screenshot(0, 0, "");

        public class Local
        {
            private int _width = 1280;

            public int Width {
                get => _width;
                set {
                    if (_width == value)
                        return;
                    _width = value;
                    LiveState?.Invalidate();
                }
            }

            public int ActualWidth => Math.Max(8, Math.Min(1920, Width));

            public ILiveState<Local, ServerScreenState>? LiveState { get; set; }
        }

        [LiveStateUpdater]
        public class Updater : ILiveStateUpdater<Local, ServerScreenState>
        {
            protected IScreenshotService Screenshot { get; }

            public Updater(IScreenshotService screenshot) => Screenshot = screenshot;

            public virtual async Task<ServerScreenState> UpdateAsync(
                ILiveState<Local, ServerScreenState> liveState, CancellationToken cancellationToken)
            {
                var local = liveState.Local;
                var screenshot = await Screenshot.GetScreenshotAsync(local.ActualWidth, cancellationToken);
                return new ServerScreenState() { Screenshot = screenshot };
            }
        }
    }
}
