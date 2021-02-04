using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Samples.BoardGames.Abstractions;
using Stl.Fusion.Authentication;
using Stl.Fusion.Server;

namespace Samples.BoardGames.Host.Controllers
{
    [Route("api/[controller]")]
    [ApiController, JsonifyErrors]
    public class GameUserController : ControllerBase, IGameUserService
    {
        protected IGameUserService GameUsers { get; }
        protected ISessionResolver SessionResolver { get; }

        public GameUserController(IGameUserService gameUsers, ISessionResolver sessionResolver)
        {
            GameUsers = gameUsers;
            SessionResolver = sessionResolver;
        }

        // Queries

        [HttpGet("find/{id}"), Publish]
        public Task<GameUser?> FindAsync([FromRoute] long id, CancellationToken cancellationToken = default)
            => GameUsers.FindAsync(id, cancellationToken);
    }
}
