using System.Threading;
using System.Threading.Tasks;
using Stl.Fusion;

namespace Samples.HelloWorld
{
    public class GreetingService
    {
        private readonly UserService _users;

        public GreetingService(UserService users) => _users = users;

        [ComputeMethod]
        public virtual async Task<string> GreetUserAsync(long userId, CancellationToken cancellationToken = default)
        {
            var user = await _users.GetUserAsync(userId, cancellationToken);
            return $"Hello, {user.Name}!";
        }
    }
}
