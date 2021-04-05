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
        [Get("getTime")]
        Task<DateTime> GetTime(CancellationToken cancellationToken = default);
        [Get("getUptime")]
        Task<TimeSpan> GetUptime(TimeSpan updatePeriod, CancellationToken cancellationToken = default);
    }

    [RestEaseReplicaService(typeof(IScreenshotService), Scope = Scopes.ClientSideOnly)]
    [BasePath("screenshot")]
    public interface IScreenshotClient
    {
        [Get("getScreenshot")]
        Task<Screenshot> GetScreenshot(int width, CancellationToken cancellationToken = default);
    }

    [RestEaseReplicaService(typeof(IChatService), Scope = Scopes.ClientSideOnly)]
    [BasePath("chat")]
    public interface IChatClient
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

    [RestEaseReplicaService(typeof(IComposerService), Scope = Scopes.ClientSideOnly)]
    [BasePath("composer")]
    public interface IComposerClient
    {
        [Get("getComposedValue")]
        Task<ComposedValue> GetComposedValue(string? parameter,
            Session session, CancellationToken cancellationToken = default);
    }

    [RestEaseReplicaService(typeof(ISumService), Scope = Scopes.ClientSideOnly)]
    [BasePath("sum")]
    public interface ISumClient
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
    
    [RestEaseReplicaService(typeof(IBoardService), Scope = Scopes.ClientSideOnly)]
    [BasePath("board")]
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
        Task<(bool, long)> CreatePlayerAsync(string boardId, string sessionId, bool isXPlayer, CancellationToken cancellationToken = default);
        [Post("createPlayerClone")]
        Task<Player> CreatePlayerCloneAsync(long id, string boardId, CancellationToken cancellationToken = default);

    }
}
