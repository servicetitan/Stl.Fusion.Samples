using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using AspNet.Security.OAuth.GitHub;
using Stl.DependencyInjection;
using Stl.Fusion.Authentication;

namespace Templates.Blazor3.Host.Services
{
    [Service]
    public class AuthSyncService
    {
        private IServerSideAuthService AuthService { get; }

        public AuthSyncService(IServerSideAuthService authService)
            => AuthService = authService;

        public async Task SyncAsync(ClaimsPrincipal principal, SessionInfo sessionInfo, Session session,
            CancellationToken cancellationToken = default)
        {
            var user = await AuthService.GetUserAsync(session, cancellationToken).ConfigureAwait(false);
            if (user.Identity.Name == principal.Identity?.Name)
                return;

            var authenticationType = principal.Identity?.AuthenticationType ?? "";
            if (authenticationType == "") {
                await AuthService.SignOutAsync(false, session, cancellationToken).ConfigureAwait(false);
            }
            else {
                var id = principal.Identity?.Name ?? "";
                var claims = principal.Claims.ToDictionary(c => c.Type, c => c.Value);
                var name = claims.GetValueOrDefault(GitHubAuthenticationConstants.Claims.Name) ?? "";
                user = new User(authenticationType, id, name, claims);
                await AuthService.SignInAsync(user, session, cancellationToken).ConfigureAwait(false);
                await AuthService.SaveSessionInfoAsync(sessionInfo, session, cancellationToken).ConfigureAwait(false);
            }
        }
    }
}
