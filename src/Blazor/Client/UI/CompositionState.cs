using System.Threading;
using System.Threading.Tasks;
using Stl.Fusion.UI;
using Samples.Blazor.Common.Services;

namespace Samples.Blazor.Client.UI
{
    public class CompositionState
    {
        public ComposedValue LocallyComposedValue { get; set; } = new ComposedValue();
        public ComposedValue RemotelyComposedValue { get; set; } = new ComposedValue();

        public class Local
        {
            private string _parameter = "Type something here";

            public string Parameter {
                get => _parameter;
                set {
                    if (_parameter == value)
                        return;
                    _parameter = value;
                    LiveState?.Invalidate();
                }
            }

            public ILiveState<Local, CompositionState>? LiveState { get; set; }
        }

        public class Updater : ILiveStateUpdater<Local, CompositionState>
        {
            protected ILocalComposerService LocalComposer { get; }
            protected IComposerService Composer { get; }

            public Updater(ILocalComposerService localComposer, IComposerService composer)
            {
                LocalComposer = localComposer;
                Composer = composer;
            }

            public virtual async Task<CompositionState> UpdateAsync(
                ILiveState<Local, CompositionState> liveState, CancellationToken cancellationToken)
            {
                var local = liveState.Local;
                var localValue = await LocalComposer.GetComposedValueAsync(local.Parameter, cancellationToken);
                var remoteValue = await Composer.GetComposedValueAsync(local.Parameter, cancellationToken);
                return new CompositionState() {
                    LocallyComposedValue = localValue,
                    RemotelyComposedValue = remoteValue,
                };
            }
        }
    }
}
