using System;
using System.Threading;
using System.Threading.Tasks;
using RestEase;
using Samples.Blazor.Common.Services;
using Stl.Fusion.Bridge;
using Stl.Fusion.Client;
using Stl.Fusion.Client.RestEase;

namespace Samples.Blazor.Client.Services
{
    [RestEaseReplicaService(typeof(ITimeService), Scope = Program.ClientSideScope)]
    [BasePath("time")]
    public interface ITimeClient : IRestEaseReplicaClient
    {
        [Get("get")]
        Task<DateTime> GetTimeAsync(CancellationToken cancellationToken = default);
    }

    [RestEaseReplicaService(typeof(IScreenshotService), Scope = Program.ClientSideScope)]
    [BasePath("screenshot")]
    public interface IScreenshotClient : IRestEaseReplicaClient
    {
        [Get("get")]
        Task<Screenshot> GetScreenshotAsync(int width, CancellationToken cancellationToken = default);
    }

    [RestEaseReplicaService(typeof(IChatService), Scope = Program.ClientSideScope)]
    [BasePath("chat")]
    public interface IChatClient : IRestEaseReplicaClient
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
    public interface IComposerClient : IRestEaseReplicaClient
    {
        [Get("get")]
        Task<ComposedValue> GetComposedValueAsync(string? parameter, CancellationToken cancellationToken = default);
    }
}
