using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Stl.Fusion;

namespace Samples.Blazor.Common.Services
{
    // TODO: Implement Authenticator
    public interface IAuthenticator
    {
        Task<string> GetLoginUrlAsync(CancellationToken cancellationToken = default);
        Task<bool> LogoutAsync(string clientId, CancellationToken cancellationToken = default);

        [ComputeMethod]
        Task<ClaimsPrincipal?> GetUserAsync(string clientId, CancellationToken cancellationToken = default);
    }
}
