using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Samples.Blazor.Abstractions;
using Stl.Fusion.Authentication;

namespace Samples.Blazor.UI.Services
{
    public class LocalComposerService : ILocalComposerService
    {
        protected ILogger Log { get; }
        private ITimeService TimeService { get; }
        private ISumService SumService { get; }
        private IChatService ChatService { get; }
        private IAuthService AuthService { get; }

        public LocalComposerService(
            ITimeService timeService,
            ISumService sumService,
            IChatService chatService,
            IAuthService authService,
            ILogger<LocalComposerService>? log = null)
        {
            Log = log ??= NullLogger<LocalComposerService>.Instance;
            TimeService = timeService;
            SumService = sumService;
            ChatService = chatService;
            AuthService = authService;
        }

        public virtual async Task<ComposedValue> GetComposedValue(
            string parameter, Session session, CancellationToken cancellationToken)
        {
            var chatTail = await ChatService.GetChatTail(1, cancellationToken);
            var uptime = await TimeService.GetUptime(10, cancellationToken);
            var sum = (double?) null;
            if (double.TryParse(parameter, out var value))
                sum = await SumService.GetSum(new [] { value }, true, cancellationToken);
            var lastChatMessage = chatTail.Messages.SingleOrDefault()?.Text ?? "(no messages)";
            var user = await AuthService.GetUser(session, cancellationToken);
            var activeUserCount = await ChatService.GetActiveUserCount(cancellationToken);
            return new ComposedValue() {
                Parameter = $"{parameter} - local",
                Uptime = uptime,
                Sum = sum,
                LastChatMessage = lastChatMessage,
                User = user,
                ActiveUserCount = activeUserCount
            };
        }
    }
}
