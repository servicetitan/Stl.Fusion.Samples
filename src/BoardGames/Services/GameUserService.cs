using System;
using System.Threading;
using System.Threading.Tasks;
using Samples.BoardGames.Abstractions;
using Stl.Fusion;
using Stl.Fusion.Authentication;
using Stl.Fusion.EntityFramework;

namespace Samples.BoardGames.Services
{
    [ComputeService(typeof(IGameUserService))]
    public class GameUserService : DbServiceBase<AppDbContext>, IGameUserService
    {
        protected IServerSideAuthService AuthService { get; }

        public GameUserService(IServiceProvider services, IServerSideAuthService authService) : base(services)
            => AuthService = authService;

        public virtual async Task<GameUser?> FindAsync(long id, CancellationToken cancellationToken = default)
        {
            var user = await AuthService.TryGetUserAsync(id.ToString(), cancellationToken);
            return user == null ? null : new GameUser() { Id = id, Name = user.Name };
        }
    }
}
