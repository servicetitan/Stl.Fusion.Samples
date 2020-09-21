using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Samples.Blazor.Client.Pages;
using Stl.Fusion;
using Samples.Blazor.Common.Services;
using Stl.Fusion.Authentication;

namespace Samples.Blazor.Client.Services
{
    [ComputeService(typeof(ILocalComposerService))]
    public class LocalComposerService : ILocalComposerService
    {
        protected ILogger Log { get; }
        private ITimeService TimeService { get; }
        private IChatService ChatService { get; }
        private IAuthService AuthService { get; }

        public LocalComposerService(
            ITimeService timeService,
            IChatService chatService,
            IAuthService authService,
            ILogger<LocalComposerService>? log = null)
        {
            Log = log ??= NullLogger<LocalComposerService>.Instance;
            TimeService = timeService;
            ChatService = chatService;
            AuthService = authService;
        }

        public virtual async Task<ComposedValue> GetComposedValueAsync(
            string parameter, AuthSession session, CancellationToken cancellationToken)
        {
            var chatTail = await ChatService.GetChatTailAsync(1, cancellationToken).ConfigureAwait(false);
            var time = await TimeService.GetTimeAsync(cancellationToken).ConfigureAwait(false);
            var lastChatMessage = chatTail.Messages.SingleOrDefault()?.Text ?? "(no messages)";
            var user = await AuthService.GetUserAsync(session, cancellationToken).ConfigureAwait(false);
            var activeUserCount = await ChatService.GetActiveUserCountAsync(cancellationToken).ConfigureAwait(false);
            return new ComposedValue($"{parameter} - local", time, lastChatMessage, user, activeUserCount);
        }
    }
}
