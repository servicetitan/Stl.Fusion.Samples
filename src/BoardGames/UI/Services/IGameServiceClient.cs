using System.Collections.Immutable;
using System.Threading;
using System.Threading.Tasks;
using RestEase;
using Stl.Fusion.Client;
using Samples.BoardGames.Abstractions;
using Stl.Fusion.Authentication;

namespace Samples.BoardGames.UI.Services
{
    [RestEaseReplicaService(typeof(IGameService), Scope = Program.ClientSideScope)]
    [BasePath("game")]
    public interface IGameServiceClient
    {
        // Commands
        [Post("create")]
        Task<Game> CreateAsync(Game.CreateCommand command, CancellationToken cancellationToken = default);
        [Post("join")]
        Task JoinAsync(Game.JoinCommand command, CancellationToken cancellationToken = default);
        [Post("start")]
        Task StartAsync(Game.StartCommand command, CancellationToken cancellationToken = default);
        [Post("move")]
        Task MoveAsync(Game.MoveCommand command, CancellationToken cancellationToken = default);
        [Post("edit")]
        Task EditAsync(Game.EditCommand command, CancellationToken cancellationToken = default);

        // Queries
        [Get("find/{id}")]
        Task<Game?> FindAsync([Path] string id, CancellationToken cancellationToken = default);
        [Get("listOwn")]
        Task<ImmutableList<Game>> ListOwnAsync(string? engineId, GameStage? stage, int count, Session session, CancellationToken cancellationToken = default);
        [Get("list")]
        Task<ImmutableList<Game>> ListAsync(string? engineId, GameStage? stage, int count, CancellationToken cancellationToken = default);
    }
}
