using System.Threading;
using System.Threading.Tasks;
using RestEase;
using Samples.Blazor.Common.Services;
using Stl.Fusion.Bridge;

namespace Samples.Blazor.Client.Services
{
    public interface IChatClient : IReplicaClient
    {
        // Writers
        [Post("createUser"), ReplicaServiceMethod(false)]
        Task<ChatUser> CreateUserAsync(string name, CancellationToken cancellationToken = default);
        [Post("setUserName"), ReplicaServiceMethod(false)]
        Task<ChatUser> SetUserNameAsync(long id, string name, CancellationToken cancellationToken = default);
        [Post("addMessage"), ReplicaServiceMethod(false)]
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

    public class ClientChatService : IChatService
    {
        private IChatClient Client { get; }
        public ClientChatService(IChatClient client) => Client = client;

        public Task<ChatUser> CreateUserAsync(string name, CancellationToken cancellationToken)
            => Client.CreateUserAsync(name, cancellationToken);
        public Task<ChatUser> SetUserNameAsync(long id, string name, CancellationToken cancellationToken)
            => Client.SetUserNameAsync(id, name, cancellationToken);
        public Task<ChatMessage> AddMessageAsync(long userId, string text, CancellationToken cancellationToken)
            => Client.AddMessageAsync(userId, text, cancellationToken);

        public Task<long> GetUserCountAsync(CancellationToken cancellationToken)
            => Client.GetUserCountAsync(cancellationToken);
        public Task<long> GetActiveUserCountAsync(CancellationToken cancellationToken)
            => Client.GetActiveUserCountAsync(cancellationToken);
        public Task<ChatUser> GetUserAsync(long id, CancellationToken cancellationToken)
            => Client.GetUserAsync(id, cancellationToken);
        public Task<ChatPage> GetChatTailAsync(int length, CancellationToken cancellationToken)
            => Client.GetChatTailAsync(length, cancellationToken);
        public Task<ChatPage> GetChatPageAsync(long minMessageId, long maxMessageId, CancellationToken cancellationToken)
            => Client.GetChatPageAsync(minMessageId, maxMessageId, cancellationToken);
    }
}
