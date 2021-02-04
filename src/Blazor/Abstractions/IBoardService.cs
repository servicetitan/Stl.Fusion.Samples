using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Stl.Fusion;
using Stl;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;

namespace Samples.Blazor.Abstractions
{
    public class Board : LongKeyedEntity
    {
        [Required, MaxLength(50)] public string BoardId { get; set; } = "";
        [Required] public string BoardState { get; set; } = "         ";
        [Required] public bool IsXTurn { get; set; } = true;
    }
    public class Player : LongKeyedEntity
    {
        [MaxLength(50)] public string PlayerId { get; set; } = "";
        [Required] public Board PlayerBoard { get; set; } = null!;
        [Required] public bool IsXPlayer { get; set; } = true;
        [Required] public string SessionId { get; set; } = "";
        [Required] public bool IsClone { get; set; } = false;
    }

    public interface IBoardService
    {
        // GET
        [ComputeMethod]
        Task<long> GetPlayerCountAsync(string boardId, CancellationToken cancellationToken = default);
        [ComputeMethod]
        Task<long> GetPlayerCountWithoutCloneAsync(string boardId, CancellationToken cancellationToken = default);
        [ComputeMethod]
        Task<Player> GetPlayerAsync(long id, CancellationToken cancellationToken = default);
        [ComputeMethod]
        Task<Player> GetPlayerBySessionAsync(string sessionId, CancellationToken cancellationToken = default);
        [ComputeMethod]
        Task<Board> GetBoardStateAsync(string boardId, CancellationToken cancellationToken = default);
        [ComputeMethod]
        Task<List<Player>> GetBoardPlayersAsync(string boardId, CancellationToken cancellationToken = default);
        [ComputeMethod]
        Task<Board> GetBoardAsync(string boardId, CancellationToken cancellationToken = default);

        // POST
        Task<Board> ChangeBoardStateAsync(string boardId, int squareIndex, bool turnX, CancellationToken cancellationToken = default);
        Task<Board> CreateBoardAsync(string boardId, CancellationToken cancellationToken = default);
        Task<Board> ClearBoardAsync(string boardId, CancellationToken cancellationToken = default);
        Task<(bool, long)> CreatePlayerAsync(string boardId, string sessionId, bool isXPlayer = true, CancellationToken cancellationToken = default);
        Task<Player> CreatePlayerCloneAsync(long id, string boardId, CancellationToken cancellationToken = default);
    }
}