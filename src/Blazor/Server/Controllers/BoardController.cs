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
        private readonly IBoardService _board;

        public BoardController(IBoardService board) => _board = board;

        // POST

        [HttpPost("changeBoardState")]
        public async Task<Board> ChangeBoardStateAsync(string boardId, int squareIndex, bool turnX, CancellationToken cancellationToken = default)
        {
            return await _board.ChangeBoardStateAsync(boardId, squareIndex, turnX, cancellationToken);
        }

        [HttpPost("createBoard")]
        public async Task<Board> CreateBoardAsync(string boardId, CancellationToken cancellationToken = default)
        {
            return await _board.CreateBoardAsync(boardId, cancellationToken);
        }
        
        [HttpPost("clearBoard")]
        public async Task<Board> ClearBoardAsync(string boardId, CancellationToken cancellationToken = default)
        {
            return await _board.ClearBoardAsync(boardId, cancellationToken);
        }

        [HttpPost("createPlayer")]
        public async Task<(bool, long)> CreatePlayerAsync(string boardId, string sessionId, bool isClone, CancellationToken cancellationToken = default)
        {
            return await _board.CreatePlayerAsync(boardId, sessionId, isClone, cancellationToken);
        }
        
        [HttpPost("createPlayerClone")]
        public async Task<Player> CreatePlayerCloneAsync(long id, string boardId, CancellationToken cancellationToken = default)
        {
            return await _board.CreatePlayerCloneAsync(id, boardId, cancellationToken);
        }

        // GET

        [HttpGet("getPlayerCount"), Publish]
        public Task<long> GetPlayerCountAsync(string boardId, CancellationToken cancellationToken = default)
            => _board.GetPlayerCountAsync(boardId, cancellationToken);

        [HttpGet("getPlayerCountWithoutClone"), Publish]
        public Task<long> GetPlayerCountWithoutCloneAsync(string boardId, CancellationToken cancellationToken = default)
            => _board.GetPlayerCountWithoutCloneAsync(boardId, cancellationToken);

        [HttpGet("getPlayer"), Publish]
        public Task<Player> GetPlayerAsync(long id, CancellationToken cancellationToken = default)
            => _board.GetPlayerAsync(id, cancellationToken);
        
        [HttpGet("getPlayerBySession"), Publish]
        public Task<Player> GetPlayerBySessionAsync(string sessionId, CancellationToken cancellationToken = default)
            => _board.GetPlayerBySessionAsync(sessionId, cancellationToken);

        [HttpGet("getBoardState"), Publish]
        public Task<Board> GetBoardStateAsync(string boardId, CancellationToken cancellationToken = default)
            => _board.GetBoardStateAsync(boardId, cancellationToken);

        [HttpGet("getBoardPlayers"), Publish]
        public Task<List<Player>> GetBoardPlayersAsync(string boardId, CancellationToken cancellationToken = default)
            => _board.GetBoardPlayersAsync(boardId, cancellationToken);

        [HttpGet("getBoard"), Publish]
        public Task<Board> GetBoardAsync(string boardId, CancellationToken cancellationToken = default)
            => _board.GetBoardAsync(boardId, cancellationToken);
    }
}
