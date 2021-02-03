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
        protected IGameService GameService { get; }
        protected ISessionResolver SessionResolver { get; }

        public GameController(IGameService gameService, ISessionResolver sessionResolver)
        {
            GameService = gameService;
            SessionResolver = sessionResolver;
        }

        // Commands

        [HttpPost("create")]
        public Task<Game> CreateAsync(Game.CreateCommand command, CancellationToken cancellationToken = default)
        {
            command.UseDefaultSession(SessionResolver);
            return GameService.CreateAsync(command, cancellationToken);
        }

        [HttpPost("join")]
        public Task JoinAsync(Game.JoinCommand command, CancellationToken cancellationToken = default)
        {
            command.UseDefaultSession(SessionResolver);
            return GameService.JoinAsync(command, cancellationToken);
        }

        [HttpPost("start")]
        public Task StartAsync(Game.StartCommand command, CancellationToken cancellationToken = default)
        {
            command.UseDefaultSession(SessionResolver);
            return GameService.StartAsync(command, cancellationToken);
        }

        [HttpPost("move")]
        public Task MoveAsync(Game.MoveCommand command, CancellationToken cancellationToken = default)
        {
            command.UseDefaultSession(SessionResolver);
            return GameService.MoveAsync(command, cancellationToken);
        }

        [HttpPost("edit")]
        public Task EditAsync(Game.EditCommand command, CancellationToken cancellationToken = default)
        {
            command.UseDefaultSession(SessionResolver);
            return GameService.EditAsync(command, cancellationToken);
        }

        // Queries
        [HttpGet("find/{id}")]
        public Task<Game?> FindAsync(string id, Session? session, CancellationToken cancellationToken = default)
        {
            session ??= SessionResolver.Session;
            return GameService.FindAsync(id, session, cancellationToken);
        }
    }
}
