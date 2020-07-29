using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using Stl.Fusion;

namespace HelloWorld
{
    public class UserService : IComputedService
    {
        private ConcurrentDictionary<long, User> _users = new ConcurrentDictionary<long, User>();

        [ComputedServiceMethod]
        public virtual async Task<User> GetUserAsync(long userId, CancellationToken cancellationToken = default)
            => _users[userId];

        public async Task AddOrUpdateUserAsync(User user, CancellationToken cancellationToken = default)
        {
            _users.AddOrUpdate(user.Id, id => user, (id, _) => user);
            Computed.Invalidate(() => GetUserAsync(user.Id, default));
        }
    }
}
