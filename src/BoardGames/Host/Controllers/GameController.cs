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

        public Task<Game> CreateAsync(Game.CreateCommand command, CancellationToken cancellationToken = default)
        {
            command.UseDefaultSession(SessionResolver);
            return GameService.CreateAsync(command, cancellationToken);
        }

        public Task JoinAsync(Game.JoinCommand command, CancellationToken cancellationToken = default)
        {
            command.UseDefaultSession(SessionResolver);
            return GameService.JoinAsync(command, cancellationToken);
        }

        public Task MoveAsync(Game.MoveCommand command, CancellationToken cancellationToken = default)
        {
            command.UseDefaultSession(SessionResolver);
            return GameService.MoveAsync(command, cancellationToken);
        }

        public Task EditAsync(Game.EditCommand command, CancellationToken cancellationToken = default)
        {
            command.UseDefaultSession(SessionResolver);
            return GameService.EditAsync(command, cancellationToken);
        }

        public Task<Game?> FindAsync(string id, Session? session, CancellationToken cancellationToken = default)
        {
            session ??= SessionResolver.Session;
            return GameService.FindAsync(id, session, cancellationToken);
        }
    }
}
