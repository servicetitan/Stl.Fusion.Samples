using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Stl.Fusion;
using Samples.Blazor.Common.Services;

namespace Samples.Blazor.Server.Services
{
    [ComputedService(typeof(IComposerService))]
    public class ComposerService : IComposerService, IComputedService
    {
        protected ILogger Log { get; }
        private ITimeService Time { get; }
        private IChatService Chat { get; }

        public ComposerService(
            ITimeService time,
            IChatService chat,
            ILogger<ComposerService>? log = null)
        {
            Log = log ??= NullLogger<ComposerService>.Instance;
            Time = time;
            Chat = chat;
        }

        [ComputedServiceMethod]
        public virtual async Task<ComposedValue> GetComposedValueAsync(string parameter, CancellationToken cancellationToken)
        {
            var chatTail = await Chat.GetChatTailAsync(1, cancellationToken).ConfigureAwait(false);
            var time = await Time.GetTimeAsync(cancellationToken).ConfigureAwait(false);
            var lastChatMessage = chatTail.Messages.SingleOrDefault()?.Text ?? "(no messages)";
            var activeUserCount = await Chat.GetActiveUserCountAsync(cancellationToken).ConfigureAwait(false);
            return new ComposedValue($"{parameter} - server", time, lastChatMessage, activeUserCount);
        }
    }
}
