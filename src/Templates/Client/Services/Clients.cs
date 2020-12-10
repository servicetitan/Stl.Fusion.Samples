using System;
using System.Threading;
using System.Threading.Tasks;
using RestEase;
using Templates.Blazor.Common.Services;
using Stl.Fusion.Authentication;
using Stl.Fusion.Client;

namespace Templates.Blazor.Client.Services
{
    [RestEaseReplicaService(typeof(ITimeService), Scope = Program.ClientSideScope)]
    [BasePath("time")]
    public interface ITimeClient
    {
        [Get("get")]
        Task<DateTime> GetTimeAsync(CancellationToken cancellationToken = default);
        [Get("getUptime")]
        Task<TimeSpan> GetUptimeAsync(TimeSpan updatePeriod, CancellationToken cancellationToken = default);
    }

    [RestEaseReplicaService(typeof(IScreenshotService), Scope = Program.ClientSideScope)]
    [BasePath("screenshot")]
    public interface IScreenshotClient
    {
        [Get("get")]
        Task<Screenshot> GetScreenshotAsync(int width, CancellationToken cancellationToken = default);
    }

    [RestEaseReplicaService(typeof(IChatService), Scope = Program.ClientSideScope)]
    [BasePath("chat")]
    public interface IChatClient
    {
        // Writers
        [Post("createUser")]
        Task<ChatUser> CreateUserAsync(string name, CancellationToken cancellationToken = default);
        [Post("setUserName")]
        Task<ChatUser> SetUserNameAsync(long id, string name, CancellationToken cancellationToken = default);
        [Post("addMessage")]
        Task<ChatMessage> AddMessageAsync(long userId, string text, CancellationToken cancellationToken = default);

        // Readers
        [Get("getUserCount")]
        Task<long> GetUserCountAsync(CancellationToken cancellationToken = default);
        [Get("getActiveUserCount")]
        Task<long> GetActiveUserCountAsync(CancellationToken cancellationToken = default);
        [Get("getUser")]
        Task<ChatUser> GetUserAsync(long id, CancellationToken cancellationToken = default);
        [Get("getChatTail")]
        Task<ChatPage> GetChatTailAsync(int length, CancellationToken cancellationToken = default);
        [Get("getChatPage")]
        Task<ChatPage> GetChatPageAsync(long minMessageId, long maxMessageId, CancellationToken cancellationToken = default);
    }

    [RestEaseReplicaService(typeof(IComposerService), Scope = Program.ClientSideScope)]
    [BasePath("composer")]
    public interface IComposerClient
    {
        [Get("get")]
        Task<ComposedValue> GetComposedValueAsync(string? parameter,
            Session session, CancellationToken cancellationToken = default);
    }

    [RestEaseReplicaService(typeof(ISumService), Scope = Program.ClientSideScope)]
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
