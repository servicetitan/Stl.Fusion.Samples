using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using AspNet.Security.OAuth.GitHub;
using Stl.DependencyInjection;
using Stl.Fusion.Authentication;

namespace Samples.Blazor.Server.Services
{
    [Service]
    public class AuthSyncService
    {
        private IServerAuthService AuthService { get; }

        public AuthSyncService(IServerAuthService authService)
            => AuthService = authService;

        public async Task SyncAsync(ClaimsPrincipal principal, AuthSession session, CancellationToken cancellationToken = default)
        {
            var user = await AuthService.GetUserAsync(session, cancellationToken).ConfigureAwait(false);
            if (user.Identity.Name == principal.Identity.Name)
                return;

            var authenticationType = principal.Identity.AuthenticationType ?? "";
            if (authenticationType == "") {
                await AuthService.LogoutAsync(session, cancellationToken).ConfigureAwait(false);
            }
            else {
                var id = principal.Identity.Name ?? "";
                var claims = principal.Claims.ToDictionary(c => c.Type, c => c.Value);
                var name = claims.GetValueOrDefault(GitHubAuthenticationConstants.Claims.Name) ?? "";
                user = new AuthUser(authenticationType, id, name, claims);
                await AuthService.LoginAsync(user, session, cancellationToken).ConfigureAwait(false);
            }
        }
    }
}
