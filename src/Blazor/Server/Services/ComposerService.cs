using Samples.Blazor.Abstractions;
using Stl.Fusion.Authentication;

namespace Samples.Blazor.Server.Services;

public class ComposerService : IComposerService
{
    protected ILogger Log { get; }
    private ITimeService TimeService { get; }
    private ISumService SumService { get; }
    private IChatService ChatService { get; }
    private IAuth Auth { get; }

    public ComposerService(
        ITimeService timeService,
        ISumService sumService,
        IChatService chatService,
        IAuth auth,
        ILogger<ComposerService>? log = null)
    {
        Log = log ?? NullLogger<ComposerService>.Instance;
        TimeService = timeService;
        SumService = sumService;
        ChatService = chatService;
        Auth = auth;
    }

    public virtual async Task<ComposedValue> GetComposedValue(
        Session session, string parameter,
        CancellationToken cancellationToken)
    {
        var chatTail = await ChatService.GetChatTail(1, cancellationToken);
        var uptime = await TimeService.GetUptime(10, cancellationToken);
        var sum = (double?) null;
        if (double.TryParse(parameter, out var value))
            sum = await SumService.GetSum(new [] { value }, true, cancellationToken);
        var lastChatMessage = chatTail.Messages.SingleOrDefault()?.Text ?? "(no messages)";
        var user = await Auth.GetUser(session, cancellationToken);
        var activeUserCount = await ChatService.GetActiveUserCount(cancellationToken);
        return new ComposedValue() {
            Parameter = $"{parameter} - server",
            Uptime = uptime,
            Sum = sum,
            LastChatMessage = lastChatMessage,
            User = user,
            ActiveUserCount = activeUserCount
        };
    }
}
