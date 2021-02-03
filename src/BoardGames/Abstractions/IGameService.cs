using System.Threading;
using System.Threading.Tasks;
using Stl.CommandR.Configuration;
using Stl.Fusion;
using Stl.Fusion.Authentication;

namespace Samples.BoardGames.Abstractions
{
    public interface IGameService
    {
        // Commands
        [CommandHandler]
        Task<Game> CreateAsync(Game.CreateCommand command, CancellationToken cancellationToken = default);
        [CommandHandler]
        Task JoinAsync(Game.JoinCommand command, CancellationToken cancellationToken = default);
        [CommandHandler]
        Task StartAsync(Game.StartCommand command, CancellationToken cancellationToken = default);
        [CommandHandler]
        Task MoveAsync(Game.MoveCommand command, CancellationToken cancellationToken = default);
        [CommandHandler]
        Task EditAsync(Game.EditCommand command, CancellationToken cancellationToken = default);

        // Queries
        [ComputeMethod(KeepAliveTime = 1)]
        Task<Game?> FindAsync(string id, Session session, CancellationToken cancellationToken = default);
    }
}
