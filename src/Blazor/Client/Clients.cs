using RestEase;
using Samples.Blazor.Abstractions;

namespace Samples.Blazor.Client;

[BasePath("time")]
public interface ITimeClientDef
{
    [Get("getTime")]
    Task<DateTime> GetTime(CancellationToken cancellationToken = default);
    [Get("getUptime")]
    Task<double> GetUptime(double updatePeriod, CancellationToken cancellationToken = default);
}

[BasePath("screenshot")]
public interface IScreenshotClientDef
{
    [Get("getScreenshot")]
    Task<Screenshot> GetScreenshot(int width, CancellationToken cancellationToken = default);
}

[BasePath("chat")]
public interface IChatClientDef
{
    // Commands
    [Post("post")]
    Task<ChatMessage> Post([Body] IChatService.PostCommand command, CancellationToken cancellationToken = default);

    // Queries
    [Get("getCurrentUser")]
    Task<ChatUser> GetCurrentUser(Session? session, CancellationToken cancellationToken = default);
    [Get("getUser")]
    Task<ChatUser> GetUser(long id, CancellationToken cancellationToken = default);
    [Get("getUserCount")]
    Task<long> GetUserCount(CancellationToken cancellationToken = default);
    [Get("getActiveUserCount")]
    Task<long> GetActiveUserCount(CancellationToken cancellationToken = default);
    [Get("getChatTail")]
    Task<ChatPage> GetChatTail(int length, CancellationToken cancellationToken = default);
    [Get("getChatPage")]
    Task<ChatPage> GetChatPage(long minMessageId, long maxMessageId, CancellationToken cancellationToken = default);
}

[BasePath("composer")]
public interface IComposerClientDef
{
    [Get("getComposedValue")]
    Task<ComposedValue> GetComposedValue(string? parameter,
        Session session, CancellationToken cancellationToken = default);
}

[BasePath("sum")]
public interface ISumClientDef
{
    // Commands
    [Post("reset")]
    Task Reset(CancellationToken cancellationToken);
    [Post("accumulate")]
    Task Accumulate(double value, CancellationToken cancellationToken);

    // Queries
    [Get("getAccumulator")]
    Task<double> GetAccumulator(CancellationToken cancellationToken);
    [Get("getSum")]
    Task<double> GetSum(double[] values, bool addAccumulator, CancellationToken cancellationToken);
}
