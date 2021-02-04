using System;
using System.Threading;
using System.Threading.Tasks;
using RestEase;
using Samples.Blazor.Abstractions;
using Stl.Fusion.Authentication;
using Stl.Fusion.Client;

namespace Samples.Blazor.Client
{
    [RestEaseReplicaService(typeof(ITimeService), Scope = Scopes.ClientSideOnly)]
    [BasePath("time")]
    public interface ITimeClient
    {
        [Get("get")]
        Task<DateTime> GetTimeAsync(CancellationToken cancellationToken = default);
        [Get("getUptime")]
        Task<TimeSpan> GetUptimeAsync(TimeSpan updatePeriod, CancellationToken cancellationToken = default);
    }

    [RestEaseReplicaService(typeof(IScreenshotService), Scope = Scopes.ClientSideOnly)]
    [BasePath("screenshot")]
    public interface IScreenshotClient
    {
        [Get("get")]
        Task<Screenshot> GetScreenshotAsync(int width, CancellationToken cancellationToken = default);
    }

    [RestEaseReplicaService(typeof(IChatService), Scope = Scopes.ClientSideOnly)]
    [BasePath("chat")]
    public interface IChatClient
    {
        // Commands
        [Post("postMessage")]
        Task<ChatMessage> PostMessageAsync([Body] IChatService.PostMessageCommand command, CancellationToken cancellationToken = default);

        // Readers
        [Get("getCurrentUser")]
        Task<ChatUser> GetCurrentUserAsync(Session? session, CancellationToken cancellationToken = default);
        [Get("getUser")]
        Task<ChatUser> GetUserAsync(long id, CancellationToken cancellationToken = default);
        [Get("getUserCount")]
        Task<long> GetUserCountAsync(CancellationToken cancellationToken = default);
        [Get("getActiveUserCount")]
        Task<long> GetActiveUserCountAsync(CancellationToken cancellationToken = default);
        [Get("getChatTail")]
        Task<ChatPage> GetChatTailAsync(int length, CancellationToken cancellationToken = default);
        [Get("getChatPage")]
        Task<ChatPage> GetChatPageAsync(long minMessageId, long maxMessageId, CancellationToken cancellationToken = default);
    }

    [RestEaseReplicaService(typeof(IComposerService), Scope = Scopes.ClientSideOnly)]
    [BasePath("composer")]
    public interface IComposerClient
    {
        [Get("get")]
        Task<ComposedValue> GetComposedValueAsync(string? parameter,
            Session session, CancellationToken cancellationToken = default);
    }

    [RestEaseReplicaService(typeof(ISumService), Scope = Scopes.ClientSideOnly)]
    [BasePath("sum")]
    public interface ISumClient
    {
        [Post("reset")]
        Task ResetAsync(CancellationToken cancellationToken);
        [Post("accumulate")]
        Task AccumulateAsync(double value, CancellationToken cancellationToken);
        [Get("getAccumulator")]
        Task<double> GetAccumulatorAsync(CancellationToken cancellationToken);
        [Get("sum")]
        Task<double> SumAsync(double[] values, bool addAccumulator, CancellationToken cancellationToken);
    }
}
