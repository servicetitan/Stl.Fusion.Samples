using System.Threading;
using System.Threading.Tasks;
using Stl.Fusion;

namespace Samples.HelloWorld
{
    public class GreetingService
    {
        private readonly UserService _users;
        private readonly TimeService _time;

        public GreetingService(UserService users, TimeService time)
        {
            _users = users;
            _time = time;
        }

        [ComputeMethod]
        public virtual async Task<string> GreetUserAsync(long userId, CancellationToken cancellationToken = default)
        {
            var user = await _users.GetUserAsync(userId, cancellationToken).ConfigureAwait(false);
            var now = await _time.GetTimeAsync(cancellationToken).ConfigureAwait(false);
            return $"Hello, {user.Name}, the time is {now.ToLongTimeString()}!";
        }
    }
}
