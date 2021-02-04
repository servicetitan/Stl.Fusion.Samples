using System.Threading;
using System.Threading.Tasks;
using Stl.Fusion;

namespace Samples.BoardGames.Abstractions
{
    public interface IGameUserService
    {
        // Queries
        [ComputeMethod(KeepAliveTime = 10)]
        Task<GameUser?> FindAsync(long id, CancellationToken cancellationToken = default);
    }
}
