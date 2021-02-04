using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Stl.Fusion.Server;
using Samples.Blazor.Abstractions;

namespace Samples.Blazor.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController, JsonifyErrors]
    public class BoardController : ControllerBase, IBoardService
    {
        private readonly IBoardService _boardService;

        public BoardController(IBoardService board) => _boardService = board;

        // POST

        [HttpPost("changeBoardState")]
        public async Task<Board> ChangeBoardStateAsync(string boardId, int squareIndex, bool turnX, CancellationToken cancellationToken = default)
            => await _boardService.ChangeBoardStateAsync(boardId, squareIndex, turnX, cancellationToken);

        [HttpPost("createBoard")]
        public async Task<Board> CreateBoardAsync(string? boardId, CancellationToken cancellationToken = default)
        {
            boardId ??= "";
            return await _boardService.CreateBoardAsync(boardId, cancellationToken);
        }

        [HttpPost("clearBoard")]
        public async Task<Board> ClearBoardAsync(string boardId, CancellationToken cancellationToken = default)
            => await _boardService.ClearBoardAsync(boardId, cancellationToken);

        [HttpPost("createPlayer")]
        public async Task<(bool, long)> CreatePlayerAsync(string boardId, string sessionId, bool isXPlayer, CancellationToken cancellationToken = default)
            => await _boardService.CreatePlayerAsync(boardId, sessionId, isXPlayer, cancellationToken);
        
        [HttpPost("createPlayerClone")]
        public async Task<Player> CreatePlayerCloneAsync(long id, string boardId, CancellationToken cancellationToken = default)
            => await _boardService.CreatePlayerCloneAsync(id, boardId, cancellationToken);

        // GET

        [HttpGet("getPlayerCount"), Publish]
        public Task<long> GetPlayerCountAsync(string boardId, CancellationToken cancellationToken = default)
            => _boardService.GetPlayerCountAsync(boardId, cancellationToken);

        [HttpGet("getPlayerCountWithoutClone"), Publish]
        public Task<long> GetPlayerCountWithoutCloneAsync(string boardId, CancellationToken cancellationToken = default)
            => _boardService.GetPlayerCountWithoutCloneAsync(boardId, cancellationToken);

        [HttpGet("getPlayer"), Publish]
        public Task<Player> GetPlayerAsync(long id, CancellationToken cancellationToken = default)
            => _boardService.GetPlayerAsync(id, cancellationToken);
        
        [HttpGet("getPlayerBySession"), Publish]
        public Task<Player> GetPlayerBySessionAsync(string sessionId, CancellationToken cancellationToken = default)
            => _boardService.GetPlayerBySessionAsync(sessionId, cancellationToken);

        [HttpGet("getBoardState"), Publish]
        public Task<Board> GetBoardStateAsync(string boardId, CancellationToken cancellationToken = default)
            => _boardService.GetBoardStateAsync(boardId, cancellationToken);

        [HttpGet("getBoardPlayers"), Publish]
        public Task<List<Player>> GetBoardPlayersAsync(string boardId, CancellationToken cancellationToken = default)
            => _boardService.GetBoardPlayersAsync(boardId, cancellationToken);

        [HttpGet("getBoard"), Publish]
        public Task<Board> GetBoardAsync(string boardId, CancellationToken cancellationToken = default)
            => _boardService.GetBoardAsync(boardId, cancellationToken);
    }
}
