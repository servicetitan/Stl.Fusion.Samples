using System.Threading;
using System.Threading.Tasks;
using RestEase;
using Samples.BoardGames.Abstractions;
using Stl.Fusion.Client;

namespace Samples.BoardGames.UI.Services
{
    [RestEaseReplicaService(typeof(IGameUserService), Scope = Program.ClientSideScope)]
    [BasePath("gameUser")]
    public interface IGameUserServiceClient
    {
        // Queries
        [Get("find/{id}")]
        Task<GameUser?> FindAsync([Path] long id, CancellationToken cancellationToken = default);
    }
}
