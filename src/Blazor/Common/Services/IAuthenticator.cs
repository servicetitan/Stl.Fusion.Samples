using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Stl.Fusion;

namespace Samples.Blazor.Common.Services
{
    // TODO: Implement this
    public interface IAuthenticator
    {
        Task<bool> LogoutAsync(string sessionId, CancellationToken cancellationToken = default);

        [ComputeMethod]
        Task<ClaimsPrincipal?> GetUserAsync(string sessionId, CancellationToken cancellationToken = default);
        [ComputeMethod]
        Task<string> GetLoginUrlAsync(string sessionId, CancellationToken cancellationToken = default);
    }
}
