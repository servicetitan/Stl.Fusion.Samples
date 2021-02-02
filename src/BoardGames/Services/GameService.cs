using System;
using System.Threading;
using System.Threading.Tasks;
using Stl.Fusion;
using Samples.BoardGames.Abstractions;
using Stl.Fusion.Authentication;

namespace Samples.BoardGames.Services
{
    [ComputeService(typeof(IGameService))]
    public class GameService : IGameService
    {
        public Task<Game> CreateAsync(Game.CreateCommand command, CancellationToken cancellationToken = default) => throw new NotImplementedException();

        public Task JoinAsync(Game.JoinCommand command, CancellationToken cancellationToken = default) => throw new NotImplementedException();

        public Task MoveAsync(Game.MoveCommand command, CancellationToken cancellationToken = default) => throw new NotImplementedException();

        public Task EditAsync(Game.EditCommand command, CancellationToken cancellationToken = default) => throw new NotImplementedException();

        public Task<Game?> FindAsync(string id, Session session, CancellationToken cancellationToken = default) => throw new NotImplementedException();
    }
}
