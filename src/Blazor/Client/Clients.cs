using System;
using System.Collections.Generic;
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
    
    [RestEaseReplicaService(typeof(IBoardService), Scope = Scopes.ClientSideOnly)]
    [BasePath("game")]
    public interface IBoardClient
    {
        // GET
        [Get("getPlayerCount")]
        Task<long> GetPlayerCountAsync(string boardId, CancellationToken cancellationToken = default);
        [Get("getPlayerCountWithoutClone")]
        Task<long> GetPlayerCountWithoutCloneAsync(string boardId, CancellationToken cancellationToken = default);
        [Get("getPlayer")]
        Task<Player> GetPlayerAsync(long id, CancellationToken cancellationToken = default);
        [Get("getPlayerBySession")]
        Task<Player> GetPlayerBySessionAsync(string sessionId, CancellationToken cancellationToken = default);
        [Get("getBoardState")]
        Task<Board> GetBoardStateAsync(string boardId, CancellationToken cancellationToken = default);
        [Get("getBoardPlayers")]
        Task<Board> GetBoardPlayersAsync(string boardId, CancellationToken cancellationToken = default);
        [Get("getBoard")]
        Task<Board> GetBoardAsync(string boardId, CancellationToken cancellationToken = default);
        
        // POST
        [Post("changeBoardState")]
        Task<Board> ChangeBoardStateAsync(string boardId, int squareIndex, bool turnX, CancellationToken cancellationToken = default);
        [Post("createBoard")]
        Task<Board> CreateBoardAsync(string boardId, CancellationToken cancellationToken = default);
        [Post("clearBoard")]
        Task<Board> ClearBoardAsync(string boardId, CancellationToken cancellationToken = default);
        [Post("createPlayer")]        
        Task<(bool, long)> CreatePlayerAsync(string boardId, string sessionId, bool isClone, CancellationToken cancellationToken = default);
        [Post("createPlayerClone")]
        Task<Player> CreatePlayerCloneAsync(long id, string boardId, CancellationToken cancellationToken = default);

    }
}
