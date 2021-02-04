using System.Collections.Immutable;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Stl.Fusion.Server;
using Stl.Fusion.Authentication;
using Samples.BoardGames.Abstractions;

namespace Samples.BoardGames.Host.Controllers
{
    [Route("api/[controller]")]
    [ApiController, JsonifyErrors]
    public class GameController : ControllerBase, IGameService
    {
        protected IGameService Games { get; }
        protected ISessionResolver SessionResolver { get; }

        public GameController(IGameService games, ISessionResolver sessionResolver)
        {
            Games = games;
            SessionResolver = sessionResolver;
        }

        // Commands

        [HttpPost("create")]
        public Task<Game> CreateAsync([FromBody] Game.CreateCommand command, CancellationToken cancellationToken = default)
        {
            command.UseDefaultSession(SessionResolver);
            return Games.CreateAsync(command, cancellationToken);
        }

        [HttpPost("join")]
        public Task JoinAsync([FromBody] Game.JoinCommand command, CancellationToken cancellationToken = default)
        {
            command.UseDefaultSession(SessionResolver);
            return Games.JoinAsync(command, cancellationToken);
        }

        [HttpPost("start")]
        public Task StartAsync([FromBody] Game.StartCommand command, CancellationToken cancellationToken = default)
        {
            command.UseDefaultSession(SessionResolver);
            return Games.StartAsync(command, cancellationToken);
        }

        [HttpPost("move")]
        public Task MoveAsync([FromBody] Game.MoveCommand command, CancellationToken cancellationToken = default)
        {
            command.UseDefaultSession(SessionResolver);
            return Games.MoveAsync(command, cancellationToken);
        }

        [HttpPost("edit")]
        public Task EditAsync([FromBody] Game.EditCommand command, CancellationToken cancellationToken = default)
        {
            command.UseDefaultSession(SessionResolver);
            return Games.EditAsync(command, cancellationToken);
        }

        // Queries

        [HttpGet("find/{id}"), Publish]
        public Task<Game?> FindAsync([FromRoute] string id, CancellationToken cancellationToken = default)
            => Games.FindAsync(id, cancellationToken);

        [HttpGet("listOwn"), Publish]
        public Task<ImmutableList<Game>> ListOwnAsync(
            string? engineId, GameStage? stage, int count, Session? session,
            CancellationToken cancellationToken = default)
        {
            session ??= SessionResolver.Session;
            return Games.ListOwnAsync(engineId, stage, count, session, cancellationToken);
        }

        [HttpGet("list"), Publish]
        public Task<ImmutableList<Game>> ListAsync(
            string? engineId, GameStage? stage, int count,
            CancellationToken cancellationToken = default)
            => Games.ListAsync(engineId, stage, count, cancellationToken);
    }
}
