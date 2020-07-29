using System.Threading;
using System.Threading.Tasks;
using Stl.Fusion;

namespace HelloWorld
{
    public class GreetingService : IComputedService
    {
        private UserService _users;

        public GreetingService(UserService users) => _users = users;

        [ComputedServiceMethod]
        public virtual async Task<string> GreetUserAsync(long userId, CancellationToken cancellationToken = default)
        {
            var user = await _users.GetUserAsync(userId, cancellationToken).ConfigureAwait(false);
            return $"Hello, {user.Name}!";
        }
    }
}
