using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Stl.Fusion;
using Samples.Blazor.Common.Services;
using Stl.Fusion.Authentication;

namespace Samples.Blazor.Server.Services
{
    [ComputeService(typeof(IComposerService))]
    public class ComposerService : IComposerService
    {
        protected ILogger Log { get; }
        private ITimeService TimeService { get; }
        private IChatService ChatService { get; }
        private IAuthService AuthService { get; }

        public ComposerService(
            ITimeService timeService,
            IChatService chatService,
            IAuthService authService,
            ILogger<ComposerService>? log = null)
        {
            Log = log ??= NullLogger<ComposerService>.Instance;
            TimeService = timeService;
            ChatService = chatService;
            AuthService = authService;
        }

        public virtual async Task<ComposedValue> GetComposedValueAsync(
            string parameter, AuthContext? context, CancellationToken cancellationToken)
        {
            var chatTail = await ChatService.GetChatTailAsync(1, cancellationToken).ConfigureAwait(false);
            var time = await TimeService.GetTimeAsync(cancellationToken).ConfigureAwait(false);
            var lastChatMessage = chatTail.Messages.SingleOrDefault()?.Text ?? "(no messages)";
            var user = await AuthService.GetUserAsync(context, cancellationToken).ConfigureAwait(false);
            var activeUserCount = await ChatService.GetActiveUserCountAsync(cancellationToken).ConfigureAwait(false);
            return new ComposedValue($"{parameter} - server", time, lastChatMessage, user, activeUserCount);
        }
    }
}
