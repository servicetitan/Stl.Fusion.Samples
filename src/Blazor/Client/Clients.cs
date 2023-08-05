using RestEase;
using Samples.Blazor.Abstractions;

namespace Samples.Blazor.Client;

[BasePath("time")]
public interface ITimeClientDef
{
    [Get(nameof(GetTime))]
    Task<DateTime> GetTime(CancellationToken cancellationToken = default);
    [Get(nameof(GetUptime))]
    Task<double> GetUptime(double updatePeriod, CancellationToken cancellationToken = default);
}

[BasePath("screenshot")]
public interface IScreenshotClientDef
{
    [Get(nameof(GetScreenshot))]
    Task<Screenshot> GetScreenshot(int width, CancellationToken cancellationToken = default);
}

[BasePath("chat")]
public interface IChatClientDef
{
    // Commands
    [Post(nameof(Post))]
    Task<ChatMessage> Post([Body] Chat_Post command, CancellationToken cancellationToken = default);

    // Queries
    [Get(nameof(GetChatTail))]
    Task<ChatMessageList> GetChatTail(int length, CancellationToken cancellationToken = default);
    [Get(nameof(GetUserCount))]
    Task<long> GetUserCount(CancellationToken cancellationToken = default);
    [Get(nameof(GetActiveUserCount))]
    Task<long> GetActiveUserCount(CancellationToken cancellationToken = default);
}

[BasePath("composer")]
public interface IComposerClientDef
{
    [Get(nameof(GetComposedValue))]
    Task<ComposedValue> GetComposedValue(string? parameter,
        Session session, CancellationToken cancellationToken = default);
}

[BasePath("sum")]
public interface ISumClientDef
{
    // Commands
    [Post(nameof(Reset))]
    Task Reset(CancellationToken cancellationToken);
    [Post(nameof(Accumulate))]
    Task Accumulate(double value, CancellationToken cancellationToken);

    // Queries
    [Get(nameof(GetAccumulator))]
    Task<double> GetAccumulator(CancellationToken cancellationToken);
    [Get(nameof(GetSum))]
    Task<double> GetSum(double[] values, bool addAccumulator, CancellationToken cancellationToken);
}
